using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	public class SnapAttribute : PropertyTraitAttribute
	{
		public float Number { get; private set; }
		public Vector4 Vector { get; private set; }
		public Bounds Bounds { get; private set; }
		public string SnapSource { get; private set; }

		public SnapAttribute(string snapSource) : base(ValidatePhase, InPhaseOrder)
		{
			SnapSource = snapSource;
		}

		public SnapAttribute(float snap) : base(ValidatePhase, InPhaseOrder)
		{
			Number = snap;
		}

		public SnapAttribute(int snap) : base(ValidatePhase, InPhaseOrder)
		{
			Number = snap;
		}

		public SnapAttribute(float x, float y) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector2(x, y);
		}

		public SnapAttribute(int x, int y) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector2(x, y);
		}

		public SnapAttribute(float x, float y, float z) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector3(x, y, z);
		}

		public SnapAttribute(int x, int y, int z) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector3(x, y, z);
		}

		public SnapAttribute(float x, float y, float z, float w) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector4(x, y, z, w);
		}

		public SnapAttribute(int x, int y, int width, int height) : base(ValidatePhase, InPhaseOrder)
		{
			Vector = new Vector4(x, y, width, height);
		}

		public SnapAttribute(float x, float y, float z, float width, float height, float depth) : base(ValidatePhase, InPhaseOrder)
		{
			Bounds = new Bounds(new Vector3(x, y, z), new Vector3(width, height, depth));
		}

		public SnapAttribute(int x, int y, int z, int width, int height, int depth) : base(ValidatePhase, InPhaseOrder)
		{
			Bounds = new Bounds(new Vector3(x, y, z), new Vector3(width, height, depth));
		}
	}
}