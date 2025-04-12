namespace Celeste64;

public class NPC : Actor, IHaveModels, IHaveSprites, IHavePushout, ICastPointShadow
{
	public SkinnedModel Model;

	public bool InteractEnabled = true;
	public float InteractRadius = 16;
	public Vec3 InteractHoverOffset;
	public bool IsPlayerOver;

	public bool ShowHover = false;
	public float ShowTimer = 0;

	public virtual float PushoutHeight { get; set; } = 12;
	public virtual float PushoutRadius { get; set; } = 8;
	public virtual float PointShadowAlpha { get; set; }

	public readonly string conversation;
	public readonly int variants;
	public readonly bool living;
	public Player? TalkingTo;
	public int variant = 0;

	public NPC(SkinnedTemplate model, string conversation, int variants = 0, bool living = true)
	{
		Model = new SkinnedModel(model);
		Model.Play("idle");
		Model.Transform = Matrix.CreateScale(3) * Matrix.CreateTranslation(0, 0, -1.5f);
		this.conversation = conversation;
		this.variants = variants;
		this.living = living;

		foreach (var mat in Model.Materials)
			mat.Effects = 0.70f;

		LocalBounds = new BoundingBox(Vec3.Zero + Vec3.UnitZ * 4, 8);
		InteractHoverOffset = new Vec3(0, 0, 16);
		PointShadowAlpha = 1;
	}

	public void Interact(Player player){
		TalkingTo = player;
		World.Add(new Cutscene(Conversation));
	}

	public virtual CoEnumerator Conversation(Cutscene cs)
	{
		if (living){
			yield return Co.Run(cs.MoveToDistance(TalkingTo, Position.XY(), 16));
			yield return Co.Run(cs.FaceEachOther(TalkingTo, this));
		} else {
			yield return Co.Run(cs.Face(TalkingTo, Position));
		}

		string cur_conversation = conversation;
		if (variants > 0){
			cur_conversation += (variant + 1);
			variant = (variant + 1) % variants;
		}
		yield return Co.Run(cs.Say(Loc.Lines(cur_conversation)));
	}

	public override void Update()
	{
		if (World.Camera.Frustum.Contains(WorldBounds))
			Model.Update();
		ShowTimer += Time.Delta;
	}

	public override void LateUpdate()
	{
		if (!ShowHover && IsPlayerOver)
			Audio.Play(Sfx.ui_npc_popup);
		ShowHover = IsPlayerOver;
		IsPlayerOver = false;
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (ShowHover)
		{
			var at = Vec3.Transform(InteractHoverOffset, Matrix);
			at += Vec3.UnitZ * MathF.Sin(ShowTimer * 8);
			populate.Add(Sprite.CreateBillboard(World, at, "interact", 2, Color.White) with { Post = true });
		}
	}
}
