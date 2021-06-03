using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseWork
{
	public class MatrixJSON
	{
		public int Count { get; set; }
		public List<List<int>> Matrix { get; set; } = new List<List<int>>();
		public int[] Assignments { get; set; }
		public int AssignmentsResult { get; set; }

		public void SetMatrix(List<List<int>> matrix)
		{
			Matrix = new List<List<int>>();
			for (int i = 0; i < matrix.Count; i++)
			{
				Matrix.Add(new List<int>());
				for (int j = 0; j < matrix[i].Count; j++)
				{
					Matrix[i].Add(matrix[i][j]);
				}
			}

			Count = matrix.Count;
		}
	}
}
