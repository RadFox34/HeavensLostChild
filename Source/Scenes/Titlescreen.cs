namespace Celeste64;

public class Titlescreen : Scene
{
	private readonly Batcher batch = new();
	private readonly SkinnedModel model;
	private float easing = 0;
	private Vec2 wobble;


	public Titlescreen()
	{
		MusicWav = "Titlescreen-WIP";

		model = new SkinnedModel(Assets.Models["logo-WIP"]);
		// TODO: remove this, its for testing while the animation isnt finished
		model.Rate = 5;
		model.SetLooping("Appear", false);
		model.Play("Appear", true);
	}

	public override void Update()
	{
		model.Update();
		easing = model.GetTime(0) / model.GetDuration(0);
		//if (easing != 1) Console.WriteLine(easing);

		if (Controls.Confirm.Pressed && !Game.Instance.IsMidTransition)
		{
			Audio.Play(Sfx.main_menu_first_input);
			
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new Overworld(false),
				ToBlack = new AngledWipe(),
				ToPause = true,
			});
		}

		if (Input.Keyboard.CtrlOrCommand && !Game.Instance.IsMidTransition && Settings.EnableQuickStart)
		{
			var entry = new Overworld.Entry(Assets.Levels[0], null);
			entry.Level.Enter();
		}

		if (Controls.Cancel.Pressed)
		{
			App.Exit();
		}
	}

	public override void Render(Target target)
	{
		target.Clear(Color.Black, 1, 0, ClearMask.All);

		var camFrom = new Vec3(0, -200, 60);
		var camTo = new Vec3(00, -80, 50);

		wobble += (Controls.Camera.Value - wobble) * (1 - MathF.Pow(.1f, Time.Delta));

		var camera = new Camera
		{
			Target = target,
			Position = new Vec3(0, -100, 0),
			LookAt = new Vec3(0, 0, 0),
			NearPlane = 10,
			FarPlane = 300
		};

		var state = new RenderState()
		{
			Camera = camera,
			ModelMatrix =
				Matrix.Identity *
				Matrix.CreateRotationX(wobble.Y) *
				Matrix.CreateRotationZ(wobble.X),
			Silhouette = false,
			SunDirection = -Vec3.UnitZ,
			VerticalFogColor = Color.White,
			DepthCompare = DepthCompare.Less,
			DepthMask = true,
			CutoutMode = false,
		};

		model.Render(ref state);

		// overlay
		{
			batch.SetSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
			var bounds = new Rect(0, 0, target.Width, target.Height);
			var scroll = -new Vec2(1.25f, 0.9f) * (float)(Time.Duration.TotalSeconds) * 0.05f;

			batch.PushBlend(BlendMode.Add);
			batch.PushSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.Repeat, TextureWrap.Repeat));
			batch.Image(Assets.Textures["overworld/overlay"],
				bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
				scroll + new Vec2(0, 0), scroll + new Vec2(1, 0), scroll + new Vec2(1, 1), scroll + new Vec2(0, 1),
				Color.White * 0.10f);
			batch.PopSampler();
			batch.PopBlend();
			batch.Image(Assets.Textures["overworld/vignette"],
				bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
				new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1),
				Color.White * 0.30f);

			if (easing < 1)
			{
				batch.PushBlend(BlendMode.Subtract);
				batch.Rect(bounds, Color.White * (1 - Ease.Cube.Out(easing)));
				batch.PopBlend();
			}
			else
			{
				var at = bounds.BottomRight + new Vec2(-16, -4) * Game.RelativeScale + new Vec2(0, -UI.PromptSize);
				UI.Prompt(batch, Controls.Cancel, Loc.Str("Exit"), at, out var width, 1.0f);
				at.X -= width + 8 * Game.RelativeScale;

				UI.Prompt(batch, Controls.Confirm, Loc.Str("Confirm"), at, out _, 1.0f);
				UI.Text(batch, Game.VersionString, bounds.BottomLeft + new Vec2(4, -4) * Game.RelativeScale, new Vec2(0, 1), Color.CornflowerBlue * 0.75f);
				UI.Text(batch, Game.LoaderVersion, bounds.BottomLeft + new Vec2(4, -24) * Game.RelativeScale, new Vec2(0, 1), new Color(12326399) * 0.75f);
				UI.Text(batch, "HLC v0.1", bounds.BottomLeft + new Vec2(4, -44) * Game.RelativeScale, new Vec2(0, 1), new Color(0xffffff) * 0.75f);
			}

			batch.Render(target);
			batch.Clear();
		}
	}
}
