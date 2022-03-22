﻿// =================================================================================================================================================================
/// <summary> Interpolation des angles pour chacune des articulations, selon la méthode d'interpolation utilisée (Quintic ou Spline cubique). </summary>

public class Trajectory
{
	public Trajectory(LagrangianModelManager.StrucLagrangianModel lagrangianModel, double t, int[] qi, out double[] qd)
	{
		double[] qdotd;
		double[] qddotd;
		Trajectory trajectory = new Trajectory(lagrangianModel, t, qi, out qd, out qdotd, out qddotd);
		trajectory.ToString();					// Pour enlever un warning lors de la compilation
	}

	public Trajectory(LagrangianModelManager.StrucLagrangianModel lagrangianModel, double t, int[] qi, out double[] qd, out double[] qdotd, out double[] qddotd)
	{
		// Initialisation des DDL à traiter et du nombre de ces DDL

		int n = qi.Length;

		// Initialisation des vecteurs contenant les positions, vitesses et accélérations des angles des articulations traités

		qd = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		qdotd = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		qddotd = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		for (int i = 0; i < qd.Length; i++)
		{
			qd[i] = 0;
			qdotd[i] = 0;
			qddotd[i] = 0;
		}

		// Boucle sur les DDLs à traiter

		for (int i = 0; i < n; i++)
			System.IO.File.AppendAllText(@"C:\Devel\AcroVR_Debug.txt", string.Format("{0}, {1}, {2}, {3}{4}", i, qi[i], lagrangianModel.q2[0], MainParameters.Instance.joints.nodes.Length, System.Environment.NewLine));

		for (int i = 0; i < n; i++)
		{
			int ii = qi[i] - lagrangianModel.q2[0];
			MainParameters.StrucNodes nodes = MainParameters.Instance.joints.nodes[ii];
			switch (nodes.interpolation.type)
			{
				// Interpolation de type Quintic

				case MainParameters.InterpolationType.Quintic:
					int j = 1;
					while (j < nodes.T.Length - 1 && t > nodes.T[j]) j++;
					Quintic.Eval(t, nodes.T[j - 1], nodes.T[j], nodes.Q[j - 1], nodes.Q[j], out qd[ii], out qdotd[ii], out qddotd[ii]);
					break;

				// Interpolation de type Spline cubique

				case MainParameters.InterpolationType.CubicSpline:
					double[] qva = new double[3];
					double[] q = new double[nodes.Q.Length + nodes.interpolation.slope.Length + 1];
					for (int k = 0; k < nodes.Q.Length; k++)
						q[k] = nodes.Q[k];
					for (int k = 0; k < nodes.interpolation.slope.Length; k++)
						q[k + nodes.Q.Length] = nodes.interpolation.slope[k];
					q[nodes.Q.Length + nodes.interpolation.slope.Length] = MainParameters.Instance.joints.duration;

					CubicSpline cubicSpline = new CubicSpline();
					switch (nodes.interpolation.numIntervals)
					{
						case 3:
							qva = cubicSpline.C3(t, q);
							break;
						case 4:
							qva = cubicSpline.C4(t, q);
							break;
						case 5:
							qva = cubicSpline.C5(t, q);
							break;
						case 6:
							qva = cubicSpline.C6(t, q);
							break;
						case 7:
							qva = cubicSpline.C7(t, q);
							break;
						case 8:
							qva = cubicSpline.C8(t, q);
							break;
					}
					qd[ii] = (double)qva[0];
					qdotd[ii] = (double)qva[1];
					qddotd[ii] = (double)qva[2];
					break;
			}
		}
	}
}
