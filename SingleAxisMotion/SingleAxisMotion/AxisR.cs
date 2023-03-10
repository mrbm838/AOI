using MotionBase;

namespace SingleAxisMotion
{
	public class AxisR : Base
	{
		public DrvMotor motor;

		public AxisR(string dbPath, string axisId = "M01_03", string strEName = "Axis_R", string strCName = "轴_R") : base()
		{
			SetMachineDataPath(dbPath);
			motor = new DrvECatMotor(this, 1, axisId, strEName, strCName);
		}

		public override void Cycle(ref double dbTime)
		{
			base.Cycle(ref dbTime);
		}

		public override void Stop()
		{
			motor.Stop();
			base.Stop();
		}

		public enum enHomeStep
		{
			StartHome = 0,
			HomeCompleted,
		}
		enHomeStep m_HomeStep;

		public override void HomeCycle(ref double dbTime)
		{
			m_HomeStep = (enHomeStep)m_nHomeStep;
			switch (m_HomeStep)
			{
				case enHomeStep.StartHome:
					m_bHomeCompleted = false;
					motor.DoHome();
					m_nHomeStep = (int)enHomeStep.HomeCompleted;
					break;
				case enHomeStep.HomeCompleted:
					if (motor.isIDLE())
					{
						m_bHomeCompleted = true;
						m_Status = 狀態.待命;
					}
					break;
			}
			base.HomeCycle(ref dbTime);
		}

		public bool DoHome()
		{
			int doStep = (int)enHomeStep.StartHome;
			bool bRet = DoHomeStep(doStep);
			return bRet;
		}

		public enum enStep
		{
			StepStrat = 0,

			StepCompleted,
		}
		enStep m_Step;

		public override void StepCycle(ref double dbTime)
		{
			m_Step = (enStep)m_nStep;
			switch (m_Step)
			{
				case enStep.StepStrat:

					m_nStep = (int)enStep.StepCompleted;
					break;
				case enStep.StepCompleted:
					if (motor.isIDLE())
					{
						m_Status = 狀態.待命;
					}
					break;
			}
			base.StepCycle(ref dbTime);
		}

		public bool Work()
		{
			int doStep = (int)enStep.StepStrat;
			bool bRet = DoStep(doStep);
			return bRet;
		}

		public bool AbsMove(double point, double speedRatio)
		{
			return motor.AbsMove(point, speedRatio);
		}

		public bool RecMove(double distance, double speedRatio)
		{
			return motor.AbsMove(distance, speedRatio);
		}

		public override bool LoadMachineData(string strMachinePath)
		{
			return base.LoadMachineData(strMachinePath);
		}
		public override bool LoadWorkData(string strWorkPath)
		{
			return base.LoadWorkData(strWorkPath);
		}
		public override bool isSafe(ref Base pBase, ref string strCDiscript, ref string strEDiscript, ref int ErrorCode)
		{
			return base.isSafe(ref pBase, ref strCDiscript, ref strEDiscript, ref ErrorCode);
		}
	}
}