﻿using System;
using System.Linq;
using Microsoft.Research.Oslo;
using System.Runtime.InteropServices;

// =================================================================================================================================================================
/// <summary> Exécution des calculs de simulation. </summary>

public class DoSimulation
{
	// Déclaration des pointeurs

	static IntPtr ptr_massMatrix;
	static IntPtr ptr_tau;
	static IntPtr ptr_Q;
	static IntPtr ptr_V;
	static IntPtr ptr_qddot2;
	static IntPtr ptr_matA;
	static IntPtr ptr_solX;

	public static bool modeRT = false;

	static double[] qd;
	static double[] qdotd;
	static double[] qddotd;

	public static int GetSimulation(out double[,] qOut)
	{
		// Affichage d'un message dans la boîte des messages

		AnimationF.Instance.DisplayNewMessage(false, true, string.Format(" {0} = {1:0.0} s", MainParameters.Instance.languages.Used.displayMsgSimulationTime, MainParameters.Instance.joints.duration));

		// Définir un nom racourci pour avoir accès à la structure Joints

		MainParameters.StrucJoints joints = MainParameters.Instance.joints;

		// Init_Move

		#region Init_Move

		double[] q0 = new double[joints.lagrangianModel.nDDL];
		double[] q0dot = new double[joints.lagrangianModel.nDDL];
		double[] q0dotdot = new double[joints.lagrangianModel.nDDL];
		Trajectory trajectory = new Trajectory(joints.lagrangianModel, 0, joints.lagrangianModel.q2, out q0, out q0dot, out q0dotdot);
		trajectory.ToString();                  // Pour enlever un warning lors de la compilation

		int[] rotation = new int[3] { joints.lagrangianModel.root_somersault, joints.lagrangianModel.root_tilt, joints.lagrangianModel.root_twist };
		int[] rotationS = Quintic.Sign(rotation);
		for (int i = 0; i < rotation.Length; i++) rotation[i] = Math.Abs(rotation[i]);

		int[] translation = new int[3] { joints.lagrangianModel.root_right, joints.lagrangianModel.root_foreward, joints.lagrangianModel.root_upward };
		int[] translationS = Quintic.Sign(translation);
		for (int i = 0; i < translation.Length; i++) translation[i] = Math.Abs(translation[i]);

		double rotRadians = joints.takeOffParam.rotation * (double)Math.PI / 180;

		double tilt = joints.takeOffParam.tilt;
		if (tilt == 90)                                 // La fonction Ode.RK547M donne une erreur fatale, si la valeur d'inclinaison est de 90 ou -90 degrés
			tilt = 90.001f;
		else if (tilt == -90)
			tilt = -90.01f;
		q0[Math.Abs(joints.lagrangianModel.root_tilt) - 1] = tilt * (double)Math.PI / 180;                                           // en radians
		q0[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = rotRadians;                                                      // en radians
		q0dot[Math.Abs(joints.lagrangianModel.root_foreward) - 1] = joints.takeOffParam.anteroposteriorSpeed;                       // en m/s
		q0dot[Math.Abs(joints.lagrangianModel.root_upward) - 1] = joints.takeOffParam.verticalSpeed;                                // en m/s
		q0dot[Math.Abs(joints.lagrangianModel.root_somersault) - 1] = joints.takeOffParam.somersaultSpeed * 2 * (double)Math.PI;     // en radians/s
		q0dot[Math.Abs(joints.lagrangianModel.root_twist) - 1] = joints.takeOffParam.twistSpeed * 2 * (double)Math.PI;               // en radians/s

		// correction of linear velocity to have CGdot = qdot

		double[] Q = new double[joints.lagrangianModel.nDDL];
		for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
			Q[i] = q0[i];
		double[] tagX;
		double[] tagY;
		double[] tagZ;
		AnimationF.Instance.EvaluateTags(Q, out tagX, out tagY, out tagZ);

		double[] cg = new double[3];          // CG in initial posture
		cg[0] = tagX[tagX.Length - 1];
		cg[1] = tagY[tagX.Length - 1];
		cg[2] = tagZ[tagX.Length - 1];

		double[] u1 = new double[3];
		double[,] rot = new double[3,1];
		for (int i = 0; i < 3; i++)
		{
			u1[i] = cg[i] - q0[translation[i] - 1] * translationS[i];
			rot[i,0] = q0dot[rotation[i] - 1] * rotationS[i];
		}
		double[,] u = { { 0, -u1[2], u1[1] }, { u1[2], 0, -u1[0] }, { -u1[1], u1[0], 0 } };
		double[,] rotM = Matrix.Multiplication(u, rot);
		for (int i = 0; i < 3; i++)
		{
			q0dot[translation[i] - 1] = q0dot[translation[i] - 1] * translationS[i] + rotM[i, 0];
			q0dot[translation[i] - 1] = q0dot[translation[i] - 1] * translationS[i];
		}

		double hFeet = Math.Min(tagZ[joints.lagrangianModel.feet[0] - 1], tagZ[joints.lagrangianModel.feet[1] - 1]);
		double hHand = Math.Min(tagZ[joints.lagrangianModel.hand[0] - 1], tagZ[joints.lagrangianModel.hand[1] - 1]);

		if (joints.condition < 8 && Math.Cos(rotRadians) > 0)
			q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hFeet;
		else															// bars, vault and tumbling from hands
			q0[Math.Abs(joints.lagrangianModel.root_upward) - 1] += joints.lagrangianModel.hauteurs[joints.condition] - hHand;
		#endregion

		// Sim_Airborn

		#region Sim_Airborn

		AnimationF.xTFrame0 = new double[joints.lagrangianModel.nDDL * 2];
		for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
		{
			AnimationF.xTFrame0[i] = q0[i];
			AnimationF.xTFrame0[joints.lagrangianModel.nDDL + i] = q0dot[i];
		}

		Options options = new Options();
		options.InitialStep = joints.lagrangianModel.dt;

		// Extraire les données obtenues du Runge-Kutta et conserver seulement les points interpolés aux frames désirés, selon la durée et le dt utilisé

		DoSimulation.modeRT = false;
		var sol = Ode.RK547M(0, joints.duration + joints.lagrangianModel.dt, new Vector(AnimationF.xTFrame0), ShortDynamics, options);
		var points = sol.SolveFromToStep(0, joints.duration + joints.lagrangianModel.dt, joints.lagrangianModel.dt).ToArray();

		double[,] q = new double[joints.lagrangianModel.nDDL, points.GetUpperBound(0) + 1];
        for (int i = 0; i < joints.lagrangianModel.nDDL; i++)
			for (int j = 0; j <= points.GetUpperBound(0); j++)
				q[i,j] = points[j].X[i];
		#endregion

		// Vérifier s'il y a un contact avec le sol

		int index = 0;
		for (int i = 0; i <= q.GetUpperBound(1); i++)
		{
			index++;
			double[] qq = new double[joints.lagrangianModel.nDDL];
			for (int j = 0; j < joints.lagrangianModel.nDDL; j++)
				qq[j] = q[j, i];
			AnimationF.Instance.EvaluateTags(qq, out tagX, out tagY, out tagZ);
			if (joints.condition > 0 && tagZ.Min() < -0.05f && AnimationF.Instance.playMode != MainParameters.Instance.languages.Used.animatorPlayModeGesticulation)
				break;
		}

		// Copier les q dans une autre matrice qOut, mais contient seulement les données jusqu'au contact avec le sol
		// Utiliser seulement pour calculer la dimension du volume utilisé pour l'animation

		qOut = new double[MainParameters.Instance.joints.lagrangianModel.nDDL, index];
        for (int i = 0; i < index; i++)
            for (int j = 0; j < MainParameters.Instance.joints.lagrangianModel.nDDL; j++)
                qOut[j, i] = (double)q[j, i];

		return points.GetUpperBound(0) + 1;
	}

	// =================================================================================================================================================================
	/// <summary> Routine qui sera exécuter par le ODE (Ordinary Differential Equation). </summary>

	public static double[] ShortDynamics_int(double ti, double[] x, double[] t, double[,] qdaa, double[,] qdotdaa, double[,] qddotdaa)
	{
		double[] q = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		double[] qdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		double[] qddot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL];
		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
		{
			q[i] = x[i];
			qdot[i] = x[MainParameters.Instance.joints.lagrangianModel.nDDL + i];
		}

		double[] qda = Quintic.Interp1(ti, t, qdaa);
		double[] qdotda = Quintic.Interp1(ti, t, qdotdaa);
		double[] qddotda = Quintic.Interp1(ti, t, qddotdaa);

		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
		{
			int ii = MainParameters.Instance.joints.lagrangianModel.q2[i] - 1;
			q[ii] = qda[ii];
			qdot[ii] = qdotda[ii];
		}

		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
			qddot[i] = qddotda[i];

		double[,] M11;
		double[,] M12;
		double[] N1;
		Inertia11Simple inertial1Simple = new Inertia11Simple();
		M11 = inertial1Simple.Inertia11(q);

		Inertia12Simple inertial2Simple = new Inertia12Simple();
		M12 = inertial2Simple.Inertia12(q);

		NLEffects1Simple nlEffect1Simple = new NLEffects1Simple();
		N1 = nlEffect1Simple.NLEffects1(q, qdot);

		// Calcul "Matrix Left division" suivante: qddot(q1) = M11\(-N1-M12*qddot(q2));
		// On peut faire ce calcul en utilisant le calcul "Matrix inverse": qddot(q1) = inv(M11)*(-N1-M12*qddot(q2));

		double[,] mA = Matrix.Inverse(M11);

		double[] qddotb = new double[MainParameters.Instance.joints.lagrangianModel.q2.Length];
		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q2.Length; i++)
			qddotb[i] = qddot[MainParameters.Instance.joints.lagrangianModel.q2[i] - 1];
		double[,] mB = Matrix.Multiplication(M12, qddotb);

		double[,] n1mB = new double[mB.GetUpperBound(0) + 1, mB.GetUpperBound(1) + 1];
		for (int i = 0; i <= mB.GetUpperBound(0); i++)
			for (int j = 0; j <= mB.GetUpperBound(1); j++)
				n1mB[i, j] = -N1[i] - mB[i, j];

		double[,] mC = Matrix.Multiplication(mA, n1mB);

		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.q1.Length; i++)
			qddot[MainParameters.Instance.joints.lagrangianModel.q1[i] - 1] = (double)mC[i, 0];

		double[] xdot = new double[MainParameters.Instance.joints.lagrangianModel.nDDL * 2];
		for (int i = 0; i < MainParameters.Instance.joints.lagrangianModel.nDDL; i++)
		{
			xdot[i] = qdot[i];
			xdot[MainParameters.Instance.joints.lagrangianModel.nDDL + i] = qddot[i];
		}

		return xdot;
	}

	public static Vector ShortDynamics(double t, Vector x)
	{
		int NDDL = MainParameters.c_nQ(MainParameters.Instance.modelBioRBDOffline);		// Récupère le nombre de DDL du modèle BioRBD
		int NROOT = 6;																	// On admet que la racine possède 6 ddl
		int NDDLhumans = 12;
		double[] xBiorbd = new double[NDDL * 2];

		double[] Qintegrateur = new double[NDDL];
		double[] Vintegrateur = new double[NDDL];
		double[] m_taud = new double[NDDL];
		double[] massMatrix = new double[NDDL * NDDL];

		double[] qddot2 = new double[NDDL];
		double[] qddot1integ = new double[NDDL * 2];
		double[] qddot1integHumans = new double[NDDLhumans];

		//Allocations des pointeurs, sinon génère erreurs de segmentation

		ptr_Q = Marshal.AllocCoTaskMem(sizeof(double) * Qintegrateur.Length);
		ptr_V = Marshal.AllocCoTaskMem(sizeof(double) * Vintegrateur.Length);
		ptr_qddot2 = Marshal.AllocCoTaskMem(sizeof(double) * qddot2.Length);
		ptr_massMatrix = Marshal.AllocCoTaskMem(sizeof(double) * massMatrix.Length);
		ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * m_taud.Length);

		// On convertit les DDL du modèle Humans pour le modèle BioRBD

		xBiorbd = ConvertHumansBioRBD.Humans2Biorbd(x);

		for (int i = 0; i < NDDL; i++)
		{
			Qintegrateur[i] = xBiorbd[i];
			Vintegrateur[i] = xBiorbd[i + NDDL];
		}

		if (!modeRT)								// Offline
		{
			double[] qdH = new double[NDDLhumans];
			double[] qdotdH = new double[NDDLhumans];
			double[] qddotdH = new double[NDDLhumans];

			Trajectory trajectory = new Trajectory(MainParameters.Instance.joints.lagrangianModel, (double)t, MainParameters.Instance.joints.lagrangianModel.q2, out qdH, out qdotdH, out qddotdH);

			qd = new double[NDDL];
			qdotd = new double[NDDL];
			qddotd = new double[NDDL];
			qd = ConvertHumansBioRBD.qValuesHumans2Biorbd(qdH);
			qdotd = ConvertHumansBioRBD.qValuesHumans2Biorbd(qdotdH);
			qddotd = ConvertHumansBioRBD.qValuesHumans2Biorbd(qddotdH);
		}

		for (int i = 0; i < qddot2.Length; i++)
			qddot2[i] = qddotd[i] + 10 * (qd[i] - Qintegrateur[i]) + 3 * (qdotd[i] - Vintegrateur[i]);
		for (int i = 0; i < NROOT; i++)
			qddot2[i] = 0;

		Marshal.Copy(Qintegrateur, 0, ptr_Q, Qintegrateur.Length);
		Marshal.Copy(Vintegrateur, 0, ptr_V, Vintegrateur.Length);
		Marshal.Copy(qddot2, 0, ptr_qddot2, qddot2.Length);

		// Génère la matrice de masse

		MainParameters.c_massMatrix(MainParameters.Instance.modelBioRBDOffline, ptr_Q, ptr_massMatrix);

		Marshal.Copy(ptr_massMatrix, massMatrix, 0, massMatrix.Length);

		MainParameters.c_inverseDynamics(MainParameters.Instance.modelBioRBDOffline, ptr_Q, ptr_V, ptr_qddot2, ptr_tau);

		Marshal.Copy(ptr_tau, m_taud, 0, m_taud.Length);

		double[,] squareMassMatrix = new double[NDDL, NDDL];
		squareMassMatrix = Matrix.FromVectorToSquare(massMatrix);            // La matrice de masse générée est sous forme d'un vecteur de taille NDDL*NDDL

		double[,] matriceA = new double[NROOT, NROOT];
		matriceA = Matrix.ShrinkSquare(squareMassMatrix, NROOT);                // On réduit la matrice de masse

		double[] matAGrandVecteur = new double[NROOT * NROOT];
		matAGrandVecteur = Matrix.FromSquareToVector(matriceA);              // La nouvelle matrice doit être convertie en vecteur pour qu'elle puisse être utilisée dans BioRBD

		ptr_matA = Marshal.AllocCoTaskMem(sizeof(double) * matAGrandVecteur.Length);
		ptr_solX = Marshal.AllocCoTaskMem(sizeof(double) * NROOT);

		Marshal.Copy(matAGrandVecteur, 0, ptr_matA, matAGrandVecteur.Length);

		MainParameters.c_solveLinearSystem(ptr_matA, NROOT, NROOT, ptr_tau, ptr_solX);  // Résouds l'équation Ax=b

		double[] solutionX = new double[NROOT];
		Marshal.Copy(ptr_solX, solutionX, 0, solutionX.Length);

		for (int i = 0; i < NROOT; i++)
			qddot2[i] = -solutionX[i];

		for (int i = 0; i < NDDL; i++)
		{
			qddot1integ[i] = Vintegrateur[i];
			qddot1integ[i + NDDL] = qddot2[i];
		}

		qddot1integHumans = ConvertHumansBioRBD.Biorbd2Humans(qddot1integ);             // Convertir les DDL du modèle BioRBD vers le modèle Humans

		// Désallocation des pointeurs

		Marshal.FreeCoTaskMem(ptr_Q);
		Marshal.FreeCoTaskMem(ptr_V);
		Marshal.FreeCoTaskMem(ptr_qddot2);
		Marshal.FreeCoTaskMem(ptr_massMatrix);
		Marshal.FreeCoTaskMem(ptr_tau);
		Marshal.FreeCoTaskMem(ptr_matA);
		Marshal.FreeCoTaskMem(ptr_solX);

		return new Vector(qddot1integHumans);
	}
}
