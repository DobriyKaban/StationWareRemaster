using Content.Client._StationWare.Points;
using Content.Shared._StationWare.Points;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Client.GameTicking.Managers;
using Robust.Shared.Timing;
using Robust.Shared.IoC;


namespace Content.Client._StationWare.ChallengeOverlay;

internal sealed class ChallengeOverlay : Overlay
{
    private readonly IPlayerManager _playerMgr;
    private readonly IEyeManager _eyeManager;

    private readonly PointSystem _point;
    private readonly ClientGameTicker _clientGameTicker;
    private readonly IGameTiming _gameTiming;

    private readonly ShaderInstance _shader;
    private readonly Font _font;
    private readonly Font _smallFont;
    private static readonly ProtoId<ShaderPrototype> UnshadedShaderId = "unshaded";

    public ChallengeOverlay(IEntityManager entity, IPrototypeManager proto, IResourceCache resourceCache, IEyeManager eyeManager, IPlayerManager player)
    {
        _eyeManager = eyeManager;
        _playerMgr = player;

        _point = entity.System<PointSystem>();
        _clientGameTicker = entity.System<ClientGameTicker>();
        _gameTiming = IoCManager.Resolve<IGameTiming>();

        ZIndex = 200;
        _shader = proto.Index(UnshadedShaderId).Instance();
        _smallFont = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 10);
        _font = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Bold.ttf"), 14);
    }

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public static bool DisplayChallengeText;
    public static string ChallengeText = string.Empty;
    public static Color TextColor = Color.White;

    public static Color ChallengeBoxColor = new(0.05f, 0.05f, 0.05f, 0.85f);

    public void UpdateText(string text, bool display, Color textColor)
    {
        DisplayChallengeText = display;
        ChallengeText = text;
        TextColor = textColor;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var aabb = args.WorldAABB;
        var width = args.ViewportBounds.Width;
        var height = args.ViewportBounds.Height;
        var scale = 2f;

        args.ScreenHandle.UseShader(_shader);

        if (!_clientGameTicker.IsGameStarted)
        {
            var difference = _clientGameTicker.StartTime - _gameTiming.CurTime;
            var seconds = Math.Max(0, (int) Math.Ceiling(difference.TotalSeconds));

            var timeVal = _gameTiming.RealTime.TotalSeconds * 2.0;
            var r = 0.5f + 0.5f * (float)Math.Sin(timeVal);
            var g = 0.3f + 0.3f * (float)Math.Cos(timeVal);
            var b = 0.8f + 0.2f * (float)Math.Sin(timeVal + 1.0);
            var textColor = new Color(r, g, b, 1.0f);

            var countdownScale = 1.8f;
            var infoScale = 1.2f;

            var timeText = Loc.GetString("lobby-state-round-start-countdown-text", ("timeLeft", seconds.ToString()));
            var modeText = Loc.GetString("lobby-hud-game-mode", ("mode", _clientGameTicker.PresetTitle));
            var mapText = Loc.GetString("lobby-hud-map", ("map", _clientGameTicker.SelectedMapTitle));
            var playersText = Loc.GetString("lobby-hud-players", ("ready", _clientGameTicker.ReadyCount), ("total", _clientGameTicker.PlayerCount));

            var timeDim = args.ScreenHandle.GetDimensions(_font, timeText, countdownScale);
            var modeDim = args.ScreenHandle.GetDimensions(_font, modeText, infoScale);
            var mapDim = args.ScreenHandle.GetDimensions(_font, mapText, infoScale);
            var playersDim = args.ScreenHandle.GetDimensions(_font, playersText, infoScale);

            var maxWidth = Math.Max(timeDim.X, Math.Max(modeDim.X, Math.Max(mapDim.X, playersDim.X)));
            var totalHeight = timeDim.Y + modeDim.Y + mapDim.Y + playersDim.Y + 24;

            var startY = height * 0.08f;
            var boxPadding = new Vector2(20, 14);
            var boxSize = new Vector2(maxWidth + boxPadding.X * 2, totalHeight + boxPadding.Y * 2);
            var boxCoords = new Vector2((width - boxSize.X) / 2f, startY);

            // Draw sleek dark background card
            args.ScreenHandle.DrawRect(UIBox2.FromDimensions(boxCoords, boxSize), new Color(0.06f, 0.06f, 0.09f, 0.85f));

            var currentY = startY + boxPadding.Y;

            // Draw countdown
            var timeCoords = new Vector2((width - timeDim.X) / 2f, currentY);
            args.ScreenHandle.DrawString(_font, timeCoords, timeText, countdownScale, textColor);
            currentY += timeDim.Y + 8;

            // Draw mode
            var modeCoords = new Vector2((width - modeDim.X) / 2f, currentY);
            args.ScreenHandle.DrawString(_font, modeCoords, modeText, infoScale, Color.FromHex("#EAEAEA"));
            currentY += modeDim.Y + 4;

            // Draw map
            var mapCoords = new Vector2((width - mapDim.X) / 2f, currentY);
            args.ScreenHandle.DrawString(_font, mapCoords, mapText, infoScale, Color.FromHex("#CCCCCC"));
            currentY += mapDim.Y + 4;

            // Draw players
            var playersCoords = new Vector2((width - playersDim.X) / 2f, currentY);
            args.ScreenHandle.DrawString(_font, playersCoords, playersText, infoScale, Color.FromHex("#32CD32"));
        }
        else
        {
            // display the challenge announcement text
            if (DisplayChallengeText)
            {
                var dimensions = args.ScreenHandle.GetDimensions(_font, ChallengeText, scale);
                var textCoords = _eyeManager.WorldToScreen(aabb.Center) - dimensions / 2f - new Vector2(0, height / 2f * 0.8f);

                var backgroundDimensions = dimensions * new Vector2(1.05f, 1.1f);
                var offset = new Vector2((backgroundDimensions.X - dimensions.X) / 2f, 0);
                args.ScreenHandle.DrawRect(UIBox2.FromDimensions(textCoords - offset, backgroundDimensions), ChallengeBoxColor);
                args.ScreenHandle.DrawString(_font, textCoords, ChallengeText, scale, TextColor);
            }

            // update the point count
            var local = _playerMgr.LocalPlayer?.UserId;
            StationWarePointManagerComponent? manager = null;
            if (local != null && _point.TryGetPointManager(ref manager))
            {
                var points = _point.GetPoints(local, manager);
                var text = Loc.GetString("overlay-point-display", ("points", points));
                var countDimensions = args.ScreenHandle.GetDimensions(_font, text, scale);
                var pointScreenCoordinates = _eyeManager.WorldToScreen(aabb.Center)
                                             - new Vector2(width * 0.97f / 2f, (-height / 2f + countDimensions.Y / 2) * 0.95f);
                args.ScreenHandle.DrawString(_smallFont, pointScreenCoordinates, text, scale, Color.White);
            }
        }

        args.ScreenHandle.UseShader(null);
    }
}
