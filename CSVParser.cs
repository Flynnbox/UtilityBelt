using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UtilityBelt
{
	public class CSVParser
	{
		#region Fields
		private bool mIgnoreEmptyRows = false;
		private bool mIgnoreEmptyColumns = false;
		private bool mIgnoreDuplicateHeaders = false;
		private bool mHasColumnHeaders = false;
		private bool mHasRowHeaders = false;
		private List<string> mColumnHeaders;
		private List<string> mRowHeaders;
		private string[,] mValues;
		private int mCurrentColumn;
		private int mCurrentRow;
		private int mRowCount;
		private int mColumnCount;
		private int mRowUpperBound;
		private int mColumnUpperBound;

		private const int ARRAY_DIMENSION_ROW = 0;
		private const int ARRAY_DIMENSION_COLUMN = 1;
		#endregion

		#region Properties
		/// <summary>
		/// Gets an array of column headers
		/// </summary>
		public string[] ColumnHeaders
		{
			get { return this.mColumnHeaders.ToArray(); }
		}

		/// <summary>
		/// Gets an array of row headers
		/// </summary>
		public string[] RowHeaders
		{
			get { return this.mRowHeaders.ToArray(); }
		}

		/// <summary>
		/// Gets or sets a value indicating that when navigating over rows, 
		/// empty rows for the current column will be automatically skipped;
		/// acts like a jagged array
		/// </summary>
		public bool IgnoreEmptyRows
		{
			get { return this.mIgnoreEmptyRows; }
			set { this.mIgnoreEmptyRows = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating that when navigating over columns,
		/// empty columns for the current row will be automatically skipped;
		/// acts like a jagged array
		/// </summary>
		public bool IgnoreEmptyColumns
		{
			get { return this.mIgnoreEmptyColumns; }
			set { this.mIgnoreEmptyColumns = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating that duplicate
		/// column or row header names will be ignore;
		/// if false, an exception will be thrown
		/// </summary>
		public bool IgnoreDuplicateHeaders
		{
			get { return this.mIgnoreDuplicateHeaders; }
			set { this.mIgnoreDuplicateHeaders = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating that the 
		/// underlying csv has column headers in the first row
		/// </summary>
		public bool HasColumnHeaders
		{
			get { return this.mHasColumnHeaders; }
			set { this.mHasColumnHeaders = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating that the 
		/// underlying csv has row headers in the first column
		/// </summary>
		public bool HasRowHeaders
		{
			get { return this.mHasRowHeaders; }
			set { this.mHasRowHeaders = value; }
		}

		/// <summary>
		/// Gets the current value for the row and column position.
		/// Value is null if the current row or column position is before or
		/// after the first or last column or row of values.
		/// </summary>
		/// <returns></returns>
		public string CurrentValue
		{
			get {
				if (this.mCurrentRow > -1 && this.mCurrentRow <= this.mRowUpperBound
				&& this.mCurrentColumn > -1 && this.mCurrentColumn <= this.mColumnUpperBound)
				{
					return this.mValues[this.mCurrentRow, this.mCurrentColumn];
				}
				else {
					return null;
				}
			}
		}

		/// <summary>
		/// Gets boolean indicating if current value is null or an empty string
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentValueEmpty
		{
			get { return (this.CurrentValue == null || this.CurrentValue.Length == 0);}
		}

		/// <summary>
		/// Gets boolean indicating true if current value is null
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentValueNull
		{
			get { return (this.CurrentValue == null); }
		}

		/// <summary>
		/// Gets header for the current row
		/// </summary>
		public string CurrentRowHeader
		{
			get {
				if (this.mHasRowHeaders && this.mCurrentRow >= 0 && this.mCurrentRow < this.mRowHeaders.Count)
				{
					return this.mRowHeaders[this.mCurrentRow];
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets header for the current column
		/// </summary>
		public string CurrentColumnHeader
		{
			get {
				if (this.mHasColumnHeaders && this.mCurrentColumn >= 0 && this.mCurrentColumn < this.mColumnHeaders.Count)
				{
					return this.ColumnHeaders[this.mCurrentColumn];
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the current column index
		/// </summary>
		public int CurrentColumnIndex
		{
			get { return this.mCurrentColumn; }
		}

		/// <summary>
		/// Gets the current row index
		/// </summary>
		public int CurrentRowIndex
		{
			get { return this.mCurrentRow; }
		}

		/// <summary>
		/// Gets boolean indicating if the current column is the last column
		/// </summary>
		public bool IsLastColumn
		{
			get { return this.mCurrentColumn == this.mColumnUpperBound; }
		}

		/// <summary>
		/// Gets boolean indicating if the current row is the last row
		/// </summary>
		public bool IsLastRow
		{
			get { return this.mCurrentRow == this.mRowUpperBound; }
		}

		/// <summary>
		/// Gets boolean indicating if the current column is the first column
		/// </summary>
		public bool IsFirstColumn
		{
			get { return this.mCurrentColumn == 0; }
		}

		/// <summary>
		/// Gets boolean indicating if the current row is the first row
		/// </summary>
		public bool IsFirstRow
		{
			get { return this.mCurrentRow == 0; }
		}

		/// <summary>
		/// Gets the count of rows
		/// </summary>
		public int RowCount
		{
			get { return this.mRowCount; }
		}

		/// <summary>
		/// Gets the count of columns
		/// </summary>
		public int ColumnCount
		{
			get { return this.mColumnCount; }
		}
		#endregion

		#region Lifecycle
		/// <summary>
		/// public constructor
		/// </summary>
		public CSVParser()
		{
			this.Reset();
		}

		/// <summary>
		/// Reads the contents of a CSV file into an internal data object
		/// </summary>
		/// <param name="csvFilePath"></param>
		/// <returns></returns>
		public bool ParseFile(string csvFilePath)
		{
			bool parseSucceeded = false;
			this.Reset();

			try
			{
				if (!File.Exists(csvFilePath))
				{
					throw new Exception(String.Format("File {0} does not exist.", csvFilePath));
				}

				if (!Path.GetExtension(csvFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception(String.Format("File {0} is not in Comma Separated Value (.csv) format or has wrong file extension.", csvFilePath));
				}

				//read each line of the file and parse out the empty rows (e.g. contain only '\r\n' or commas)
				StringBuilder sbData = new StringBuilder();
				Regex emptyRow = new Regex("^[,\r\n]+$", RegexOptions.Compiled);
				using (StreamReader reader = new StreamReader(csvFilePath))
				{
					while (!reader.EndOfStream)
					{
						sbData.Append(emptyRow.Replace(reader.ReadLine(), ""));
						sbData.Append("\n");
					}
				}
				string data = sbData.ToString();

				if (data != null && data.Length == 0)
				{
					throw new Exception(String.Format("File {0} contains no data that could be read.", csvFilePath));
				}

				//split the row by new line characters, preserve empty rows
				string[] rows = data.Split(new string[] { "\n" }, data.Length, StringSplitOptions.RemoveEmptyEntries);

				//iterate through the rows and parse out the columns
				int rowPosition = 0;
				foreach (string row in rows)
				{
					string newRow = row;

					//remove any preceeding or trailing commas from the column header row
					if (this.mHasColumnHeaders && rowPosition == 0)
					{
							newRow = row.Trim(new char[] { ',' });
					}

					//split the columns by comma, preserve empty columns
					string[] columns = newRow.Split(new char[] { ',' }, StringSplitOptions.None);

					//initialize interal objects on the first iteration
					if (rowPosition == 0)
					{
						//determine the number or rows and columns
						//and modify length to account for row and column headers
						// - the internal multidimensional string array of values should not include headers
						int rowHeaderLength = 0;
						int columnHeaderLength = 0;
						if (this.mHasRowHeaders)
						{
							if (this.mHasColumnHeaders)
							{
								rowHeaderLength = rows.Length - 1;
								columnHeaderLength = columns.Length - 1;
							}
							else
							{
								rowHeaderLength = rows.Length;
							}
						}
						else if(this.mHasColumnHeaders)
						{
							columnHeaderLength = columns.Length;
						}

						mRowHeaders = new List<string>(rowHeaderLength);
						mColumnHeaders = new List<string>(columnHeaderLength);
						mValues = new string[rowHeaderLength, columnHeaderLength];
					}

					//iterate through the columns and parse out the values
					int columnPosition = 0;
					foreach (string value in columns)
					{
						//if we have column headers, do not assign values beyond those we can match to column headers
						if (mHasColumnHeaders && columnPosition > mColumnHeaders.Capacity)
						{
							break;
						}

						//check if this is a rowHeader
						if (this.mHasRowHeaders && columnPosition == 0)
						{
							//don't grab first row value if this has column headers
							if (!this.mHasColumnHeaders || !(rowPosition == 0))
							{
								if (value.Length > 0)
								{
									//check that this is not a duplicate
									if (!this.mRowHeaders.Contains(value))
									{
										this.mRowHeaders.Add(value.Trim());
									}
									else if (!mIgnoreDuplicateHeaders)
									{
										throw new Exception(String.Format("Duplicate row header {0} detected at column {1}, row {2}", value, columnPosition, rowPosition));
									}
								}
							}
						}
						//check if this is a columnHeader
						else if (this.mHasColumnHeaders && rowPosition == 0)
						{
							//don't grab first column value if this has row headers
							if (!this.mHasRowHeaders || !(columnPosition == 0))
							{
								if (value.Length > 0)
								{
									//check that this is not a duplicate
									if (!this.mColumnHeaders.Contains(value))
									{
										this.mColumnHeaders.Add(value.Trim());
									}
									else if (!mIgnoreDuplicateHeaders)
									{
										throw new Exception(String.Format("Duplicate column header {0} detected at column {1}, row {2}", value, columnPosition, rowPosition));
									}
								}
							}
						}
						//read column values
						else
						{
							//insure that we capture the zero-index value position, not relative to column or row headers
							this.mValues[this.mHasColumnHeaders ? rowPosition - 1 : rowPosition,
								this.mHasRowHeaders ? columnPosition - 1 : columnPosition] = value.Trim();
						}
						columnPosition++;
					}
					rowPosition++;
				}

				//initialize some unchanging properties for the current values array
				this.mRowCount = this.mValues.GetLength(ARRAY_DIMENSION_ROW);
				this.mColumnCount = this.mValues.GetLength(ARRAY_DIMENSION_COLUMN);
				this.mRowUpperBound = this.mValues.GetUpperBound(ARRAY_DIMENSION_ROW);
				this.mColumnUpperBound = this.mValues.GetUpperBound(ARRAY_DIMENSION_COLUMN);
				parseSucceeded = true;
			}
			catch (Exception ex)
			{
				parseSucceeded = false;
				throw ex;
			}
			return parseSucceeded;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Returns false if this is last column position in Row 
		/// and resets column position before first column;
		/// else moves to next column position and returns true
		/// </summary>
		/// <param name="moveNextRow">indicates if the reader should automatically jump to the next row if no more columns exist in this row</param>
		public bool NextColumn(bool moveNextRow)
		{
			if (this.mCurrentColumn < this.mColumnUpperBound)
			{
				this.mCurrentColumn++;
			}
			else
			{
				//no more columns in this row - check if we should move to the next row
				if (moveNextRow && this.NextRow(true))
				{
					this.MoveToFirstColumn();
				}
				else
				{
					//reset column position before first column
					this.mCurrentColumn = -1;
					return false;
				}
			}

			//check if we should skip to next column
			if (this.mIgnoreEmptyColumns && this.mCurrentRow >= 0 && this.IsCurrentValueEmpty)
			{
				this.NextColumn();
			}
			return true;
		}

		/// <summary>
		/// Returns false if this is last column position in Row
		/// and resets column position before first column;
		/// else moves to next column position and returns true
		/// - does not automatically move to next row if no more columns exist
		/// </summary>
		public bool NextColumn()
		{
			return this.NextColumn(false);
		}

		/// <summary>
		/// Returns false if this is last row position in Column
		/// and resets row position before first row;
		/// else moves to next row position and returns true
		/// </summary>
		/// <param name="moveNextRow">indicates if the reader should automatically jump to the next column if no more rows exist in this column</param>
		public bool NextRow(bool moveNextColumn)
		{
			if (this.mCurrentRow < this.mRowUpperBound)
			{
				mCurrentRow++;
			}
			else
			{
				//no more columns in this row - check if we should move to the next row
				if (moveNextColumn && this.NextColumn(true))
				{
					this.MoveToFirstRow();
				}
				else
				{
					//reset row position before first row
					this.mCurrentRow = -1;
					return false;
				}
			}

			//check if we should skip to next row
			if (this.mIgnoreEmptyRows && this.mCurrentColumn >= 0 && this.IsCurrentValueEmpty)
			{
				this.NextRow();
			}
			return true;
		}

		/// <summary>
		/// Returns false if this is last row position in Column
		/// and resets row position before first row;
		/// else moves to next row position and returns true
		/// - does not automatically move to next column if no more rows exist
		/// </summary>
		public bool NextRow()
		{
			return this.NextRow(false);
		}

		/// <summary>
		/// Iterates through all values, by row, 
		/// and advances to next column of values at end of each row;
		/// returns null when no more values
		/// </summary>
		/// <returns></returns>
		public string NextValueByRow()
		{
			if (this.NextRow(true))
			{
				return this.CurrentValue;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Iterates through all values, by column, 
		/// and advances to next row of values at end of each column;
		/// returns null when no more values
		/// </summary>
		/// <returns></returns>
		public string NextValueByColumn()
		{
			if (this.NextColumn(true))
			{
				return this.CurrentValue;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Moves to the first row of values
		/// </summary>
		public void MoveToFirstRow()
		{
			this.mCurrentRow = 0;
		}

		/// <summary>
		/// Moves to the first column of values
		/// </summary>
		public void MoveToFirstColumn()
		{
			this.mCurrentColumn = 0;
		}

		/// <summary>
		/// Moves to the last row of values
		/// </summary>
		public void MoveToLastRow()
		{
			this.mCurrentRow = mRowUpperBound;
		}

		/// <summary>
		/// Moves to the last column of values
		/// </summary>
		public void MoveToLastColumn()
		{
			this.mCurrentColumn = mColumnUpperBound;
		}

		/// <summary>
		/// If csv contains row names and the supplied row name matches one of them
		/// then moves to that row position and returns true; else false.
		/// - Note that the match is case sensitive
		/// </summary>
		/// <returns></returns>
		public bool MoveToRow(string rowHeader)
		{
			if (this.mHasRowHeaders && rowHeader != null && this.mRowHeaders.Contains(rowHeader))
			{
				return this.MoveToRow(this.mRowHeaders.IndexOf(rowHeader));
			}
			return false;
		}

		/// <summary>
		/// If the index position is a valid row position, 
		/// moves to that row position and returns true;
		/// else returns false
		/// - Note that empty rows are included in the row index
		/// </summary>
		/// <returns></returns>
		public bool MoveToRow(int index)
		{
			if (index >= 0 && index <= this.mRowUpperBound)
			{
				this.mCurrentRow = index;
				return true;
			}
			return false;
		}

		/// <summary>
		/// If csv contains column names and the supplied column name matches one of them
		/// then moves to that column position and returns true; else false.
		/// - Note that the match is case sensitive
		/// </summary>
		/// <returns></returns>
		public bool MoveToColumn(string columnHeader)
		{
			if (this.mHasColumnHeaders && columnHeader != null && this.mColumnHeaders.Contains(columnHeader))
			{
				return this.MoveToColumn(this.mColumnHeaders.IndexOf(columnHeader));
			}
			return false;
		}

		/// <summary>
		/// If the index position is a valid column position, 
		/// moves to that column position and returns true;
		/// else returns false
		/// - Note that empty columns are included in the column index 
		/// </summary>
		/// <returns></returns>
		public bool MoveToColumn(int index)
		{
			if (index >= 0 && index <= this.mColumnUpperBound)
			{
				this.mCurrentColumn = index;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Moves the current position before the first row and column; no current record
		/// </summary>
		public void Reset()
		{
			this.mCurrentColumn = -1;
			this.mCurrentRow = -1;
		}
		#endregion
	}
}
