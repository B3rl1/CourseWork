using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using HungarianAlgorithm;
using OfficeOpenXml;

namespace CourseWork
{
	public partial class Form1 : Form
	{
		public const string StringJsonName = "json";
		public const string StringExcelName = "xlsx";
		public const int MaxCountOfMatrixByProgramSize = 16;

		private MatrixJSON _matrixJson = new MatrixJSON();
		private int _n = -1;
		private int[,] _matrixArr;
		private bool _isGeneratedMatrix = false;

		//TO-DO: написать в программе, что должен вводить пользователь. Как работать с программой. Привязать к физическому смыслу.

		private bool _isOpenedFile = false;
		public Form1()
		{
			InitializeComponent();
			dataGridView1.Visible = false;

			foreach (DataGridViewColumn column in dataGridView1.Columns)
			{
				column.SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			dataGridView1.CellValidating += CellValidating;
			textBox1.Validating += TextValidating;
			button1.Select();

			ToolStripMenuItem fileItem = new ToolStripMenuItem("Файл");
			
			fileItem.DropDownItems.Add("Открыть");
			fileItem.DropDownItems[0].Click += OpenJsonFile;
			fileItem.DropDownItems.Add("Сохранить");
			fileItem.DropDownItems[1].Click += SaveJsonFile;
			fileItem.DropDownItems.Add("Экспортировать");
			fileItem.DropDownItems[2].Click += ExportToExcel;

			menuStrip1.Items.Add(fileItem);

			ToolStripMenuItem aboutProgrammer = new ToolStripMenuItem("О разработчике");
			aboutProgrammer.Click += AboutProgrammer;
			menuStrip1.Items.Add(aboutProgrammer);
		}

		private void AboutProgrammer(object? sender, EventArgs e)
		{
			MessageBox.Show("Программа создана студентом 3 курса 4 группы института ЭУИС.\nСтаценко Кирилл Владимирович.\nДата выпуска: 23.05.2021", "О разработчике");
		}

		private void TextValidating(object sender, CancelEventArgs e)
		{
			int newInteger;
			if (!int.TryParse(textBox1.Text, out newInteger) || newInteger < 2)
			{
				e.Cancel = true;
				MessageBox.Show("Введите корректные данные для размерности таблицы!" +
				                "\nПример: 2,3,10,5." +
				                "\nВвод дробных и отрицательных числе невозможен");
			}
			else
			{
				_n = newInteger;
				_matrixJson.Count = _n;
				_isGeneratedMatrix = false;
			}
		}

		private void CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			int newInteger;
			if (dataGridView1.Rows[e.RowIndex].IsNewRow) { return; }
			if (!int.TryParse(e.FormattedValue.ToString(), out newInteger))
			{
				MessageBox.Show("Введите число!", "Ошибка ввода");
				dataGridView1[e.ColumnIndex, e.RowIndex].Value = "";
				e.Cancel = true;
			}
			else
			{
				_matrixJson.Matrix[e.RowIndex][e.ColumnIndex] = newInteger;
			}

			dataGridView1.ClearSelection();
		}

		private void ExportToExcel(object? sender, EventArgs e)
		{
			var package = new ExcelPackage();
			var sheet = package.Workbook.Worksheets.Add("Венгерский метод");
			sheet.Cells["B2"].Value = "Решение венгерского алгоритма";
			sheet.Cells["B3"].Value = "Исходная матрица";

			int row = 4;
			int column = 2;

			for (int i = 0; i < _matrixJson.Matrix.Count; i++)
			{
				for (int j = 0; j < _matrixJson.Matrix[i].Count; j++)
				{
					sheet.Cells[row, column+j].Value = _matrixJson.Matrix[i][j];
				}
				row++;
			}

			sheet.Cells[row++, column].Value = "Оптимальное назначение";
			sheet.Cells[row, column].Value = "Строка";
			sheet.Cells[row++, column+1].Value = "Столбец";
			for (int i = 0; i < _matrixJson.Assignments.Length; i++)
			{
				sheet.Cells[row, column].Value = i;
				sheet.Cells[row++, column+1].Value = _matrixJson.Assignments[i];
			}
			sheet.Cells[row, column].Value = "Оптимальное назначение";
			sheet.Cells[row, column+1].Value = _matrixJson.AssignmentsResult;

			SaveFileAsync(StringExcelName, package);
		}

		private async void SaveFileAsync(string extension, ExcelPackage package = null)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
				return;

			string filename;
			// получаем выбранный файл
			if (saveFileDialog1.FileName.Contains("."))
			{
				filename = saveFileDialog1.FileName.Remove(saveFileDialog1.FileName.IndexOf(".")) + "." + extension;
			}
			else
			{
				filename = saveFileDialog1.FileName + "." + extension;
			}
				

			using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
			{
				switch (extension)
				{
					case StringJsonName:
						try
						{
							await JsonSerializer.SerializeAsync(fs, _matrixJson);
						}
						catch (Exception e)
						{
							Console.WriteLine("Не удалось экспортировать в JSON файл.", "Ошибка экспортирования!");
							throw;
						}

						break;
					case StringExcelName:
						try
						{
							byte[] byteArray = package.GetAsByteArray();
							await fs.WriteAsync(byteArray, 0, byteArray.Length);
						}
						catch (Exception exception)
						{
							MessageBox.Show("Не удалось экспортировать в Excel файл.", "Ошибка экспортирования в эксель!");
						}
						break;
				}
			}
		}

		private void SaveJsonFile(object? sender, EventArgs e)
		{
			SaveFileAsync(StringJsonName);
		}

		private async void OpenJsonFile(object? sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
				return;

			string filename = openFileDialog1.FileName;
			try
			{
				using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
				{
					fs.Position = 0;
					_matrixJson = await JsonSerializer.DeserializeAsync<MatrixJSON>(fs);
					_isOpenedFile = true;
					textBox1.Text = _matrixJson.Count.ToString();
					_n = _matrixJson.Count;
					_matrixArr = new int[_matrixJson.Count, _matrixJson.Count];
					for (int i = 0; i < _matrixJson.Matrix.Count; i++)
					{
						for (int j = 0; j < _matrixJson.Matrix[i].Count; j++)
						{
							_matrixArr[i, j] = _matrixJson.Matrix[i][j];
						}
					}

					InitializeDataGrid(dataGridView1, _matrixJson.Count, _isOpenedFile);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show("Ошибка чтения JSON файла!", "Ошибка чтения");
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			InitializeDataGrid(dataGridView1, _n, _isOpenedFile);
		}

		private int GetResultByAssignments(int[] resultArray, int[,] costs)
		{
			int result = 0;
			for (int i = 0; i < resultArray.Length; i++)
			{
				result += costs[i, resultArray[i]];
			}

			return result;
		}

		private void InitializeDataGrid(DataGridView dataGrid, int n, bool isDefault = false)
		{
			if (n != -1)
			{
				dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				dataGrid.ColumnCount = n;
				dataGrid.RowCount = n;
				dataGrid.Visible = true;

				if (n < MaxCountOfMatrixByProgramSize)
					dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.None;
				else
					dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Both;

				int totalRowHeight = 0;
				dataGrid.RowHeadersWidth = dataGrid.ColumnHeadersHeight;
				int totalColumnsWidth = 0;

				Random rnd = new Random();

				for (int i = 0; i < dataGrid.Rows.Count; i++)
				{
					if (!isDefault)
					{
						_matrixJson.Matrix.Add(new List<int>());
					}

					for (int j = 0; j < dataGrid.Columns.Count; j++)
					{
						if (isDefault)
						{
							dataGrid[j, i].Value = _matrixJson.Matrix[i][j];
						}
						else
						{
							_matrixJson.Matrix[i].Add(0);
							dataGrid[j, i].Value = 0;
						}
					}
				}

				if (isDefault)
					_isOpenedFile = false;

				for (int i = 0; i < (n < MaxCountOfMatrixByProgramSize ? dataGrid.Columns.Count : MaxCountOfMatrixByProgramSize); i++)
				{
					totalRowHeight += dataGrid.Rows[i].Height;
					totalColumnsWidth += dataGrid.Columns[i].Width;
				}

				dataGrid.Height = totalRowHeight;
				dataGrid.Width = totalColumnsWidth;
				_isGeneratedMatrix = true;
			}
			else
			{
				MessageBox.Show("Для генерации таблицы введите её размерность!", "Ошибка генерации таблицы");
			}
		}

		private void button2_Click_1(object sender, EventArgs e)
		{
			if (_isGeneratedMatrix)
			{
				Random rnd = new Random();
				_matrixArr = new int[_n, _n];
				int[,] matrixTmp = new int[_n, _n];
				List<List<int>> matList = new List<List<int>>();

				for (int i = 0; i < dataGridView1.Rows.Count; i++)
				{
					matList.Add(new List<int>());
					for (int j = 0; j < dataGridView1.Columns.Count; j++)
					{
						int s = Convert.ToInt32(dataGridView1[i, j].Value);
						_matrixArr[j, i] = s;
						matrixTmp[j, i] = s;
					}
				}

				int[] result = HungarianAlgorithm.HungarianAlgorithm.FindAssignments(_matrixArr, checkBox1.Checked);

				StringBuilder builder = new StringBuilder();

				int x = 1;
				foreach (var i in result)
				{
					builder.Append($"Строка: {x++} Столбец: {i+1}" + Environment.NewLine);
				}

				int resultAssignments = GetResultByAssignments(result, matrixTmp);
				_matrixJson.Assignments = result;
				_matrixJson.AssignmentsResult = resultAssignments;

				textBox2.Text = builder.ToString();
				textBox3.Text = resultAssignments.ToString();
			}
			else
			{
				MessageBox.Show("Сначала создайте матрицу для поиска решения!", "Ошибка поиска решения!");
			}
			
		}

		private void dataGridView1_MouseLeave(object sender, EventArgs e)
		{
			dataGridView1.EndEdit();
		}
	}
}
