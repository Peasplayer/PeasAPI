using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reactor.Extensions;
using UnityEngine;

namespace PeasAPI
{
    public class Data
    {
        public readonly struct CustomIntroScreen
        {
            public readonly bool OverrideTeam;
            public readonly string Team;
            public readonly string TeamDescription;
            public readonly Color TeamColor;
            public readonly List<byte> TeamMembers;
            public readonly bool OverrideRole;
            public readonly string Role;
            public readonly string RoleDescription;
            public readonly Color RoleColor;

            public CustomIntroScreen(bool overrideTeam = false, string team = null, string teamDescription = null, Color? teamColor = null, List<byte> teamMembers = null, bool overrideRole = false, string role = null, string roleDescription= null, Color? roleColor = null)
            {
                OverrideTeam = overrideTeam;
                Team = team;
                TeamColor = teamColor.GetValueOrDefault();
                TeamDescription = teamDescription;
                TeamMembers = teamMembers;
                OverrideRole = overrideRole;
                Role = role;
                RoleDescription = roleDescription;
                RoleColor = roleColor.GetValueOrDefault();
            }
        }
        
        public readonly struct Hat
        {
            public readonly string Name;
            public readonly string ImagePath;
            public readonly Assembly Assembly;
            public readonly bool InFront;
            public readonly bool NoBounce;
            public readonly Vector2 ChipOffset;
            public readonly Sprite BackImage;
            public readonly Sprite FloorImage;
            
            public Hat(string name, string imagePath, Assembly assembly, bool inFront, bool noBounce, Vector2 chipOffset, Sprite backImage, Sprite floorImage)
            {
                Name = name;
                ImagePath = imagePath;
                Assembly = assembly;
                InFront = inFront;
                NoBounce = noBounce;
                ChipOffset = chipOffset;
                BackImage = backImage;
                FloorImage = floorImage;
            }
            
            public HatData CreateHat()
            {
                try
                {
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                    Stream myStream = Assembly.GetManifestResourceStream(ImagePath);
                    byte[] data = myStream.ReadFully();
                    ImageConversion.LoadImage(tex, data, false);

                    var newHat = ScriptableObject.CreateInstance<HatData>();
                    newHat.hatViewData.viewData = ScriptableObject.CreateInstance<HatViewData>();
                    newHat.hatViewData.viewData.MainImage = newHat.hatViewData.viewData.LeftMainImage = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newHat.ProductId = $"+{Name}";
                    newHat.displayOrder += 100;
                    newHat.Free = true;
                    newHat.StoreName = Name;
                    newHat.name = Name;
                    
                    newHat.InFront = InFront;
                    newHat.NoBounce = NoBounce;
                    newHat.ChipOffset = ChipOffset;
                    newHat.hatViewData.viewData.BackImage = newHat.hatViewData.viewData.LeftBackImage = BackImage;
                    newHat.hatViewData.viewData.ClimbImage = newHat.hatViewData.viewData.LeftClimbImage = BackImage;
                    newHat.hatViewData.viewData.FloorImage = newHat.hatViewData.viewData.LeftFloorImage = FloorImage;
                    
                    return newHat;
                }
                catch (Exception e)
                {
                    PeasAPI.Logger.LogError($"Error while creating a hat: {e}");
                }

                return null;
            }
        }
        
        public readonly struct Visor
        {
            public readonly string Name;
            public readonly string ImagePath;
            public readonly Assembly Assembly;
            public readonly Vector2 ChipOffset;
            public readonly Sprite ClimbImage;
            public readonly Sprite FloorImage;
            
            public Visor(string name, string imagePath, Assembly assembly, Vector2 chipOffset, Sprite climbImage, Sprite floorImage)
            {
                Name = name;
                ImagePath = imagePath;
                Assembly = assembly;
                ChipOffset = chipOffset;
                ClimbImage = climbImage;
                FloorImage = floorImage;
            }
            
            public VisorData CreateVisor()
            {
                try
                {
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                    Stream myStream = Assembly.GetManifestResourceStream(ImagePath);
                    byte[] data = myStream.ReadFully();
                    ImageConversion.LoadImage(tex, data, false);

                    var newVisor = ScriptableObject.CreateInstance<VisorData>();
                    newVisor.viewData.viewData = ScriptableObject.CreateInstance<VisorViewData>();
                    newVisor.viewData.viewData.IdleFrame = newVisor.viewData.viewData.LeftIdleFrame = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newVisor.ProductId = $"+{Name}";
                    newVisor.displayOrder += 100;
                    newVisor.Free = true;
                    newVisor.name = Name;
                    
                    newVisor.ChipOffset = ChipOffset;
                    newVisor.viewData.viewData.ClimbFrame = ClimbImage;
                    newVisor.viewData.viewData.FloorFrame = FloorImage;
                    
                    return newVisor;
                }
                catch (Exception e)
                {
                    PeasAPI.Logger.LogError($"Error while creating a visor: {e.StackTrace}");
                }

                return null;
            }
        }
    }
}