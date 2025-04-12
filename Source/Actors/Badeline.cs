namespace Celeste64;

public class Badeline : NPC
{
	public readonly Hair Hair;
	public virtual Color HairColor => 0x9B3FB5;

	public Badeline() : base(Assets.Models["badeline"], "Baddy", 3)
	{
		Model.Play("Bad.Idle");

		foreach (var mat in Model.Materials)
		{
			if (mat.Name == "Hair")
			{
				mat.Color = HairColor;
				mat.Effects = 0;
			}
			mat.SilhouetteColor = HairColor;
		}

		Hair = new()
		{
			Color = HairColor,
			ForwardOffsetPerNode = 0,
			Nodes = 10
		};
	}

	public override void Update()
	{
		base.Update();

		// update model
		Model.Transform =
			Matrix.CreateScale(3) *
			Matrix.CreateTranslation(0, 0, MathF.Sin(World.GeneralTimer * 2) * 1.0f - 1.5f);

		// update hair
		{
			var hairMatrix = Matrix.Identity;
			foreach (var it in Model.Instance.Armature.LogicalNodes)
				if (it.Name == "Head")
					hairMatrix = it.ModelMatrix * SkinnedModel.BaseTranslation * Model.Transform * Matrix;
			Hair.Flags = Model.Flags;
			Hair.Forward = -new Vec3(Facing, 0);
			Hair.Materials[0].Effects = 0;
			Hair.Update(hairMatrix);
		}

	}
	
	public override void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Hair));
		base.CollectModels(populate);
	}
}

