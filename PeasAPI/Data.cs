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
            public readonly string Team;
            public readonly string TeamDescription;
            public readonly Color TeamColor;
            public readonly List<byte> TeamMembers;
            public readonly string Role;
            public readonly string RoleDescription;
            public readonly Color RoleColor;

            public CustomIntroScreen(string team, string teamDescription, Color teamColor, List<byte> teamMembers, string role = null, string roleDescription= null, Color? roleColor = null)
            {
                Team = team;
                TeamColor = teamColor;
                TeamDescription = teamDescription;
                TeamMembers = teamMembers;
                Role = role.IsNullOrWhiteSpace() ? team : role;
                RoleDescription = roleDescription.IsNullOrWhiteSpace() ? teamDescription : roleDescription;
                RoleColor = roleColor ?? teamColor;
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
            
            public HatBehaviour CreateHat()
            {
                try
                {
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                    Stream myStream = Assembly.GetManifestResourceStream(ImagePath);
                    byte[] data = myStream.ReadFully();
                    ImageConversion.LoadImage(tex, data, false);

                    var newHat = ScriptableObject.CreateInstance<HatBehaviour>();
                    newHat.MainImage = newHat.LeftMainImage = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newHat.ProductId = $"+{Name}";
                    newHat.Order += 100;
                    newHat.Free = true;
                    newHat.StoreName = Name;
                    newHat.name = Name;
                    
                    newHat.InFront = InFront;
                    newHat.NoBounce = NoBounce;
                    newHat.ChipOffset = ChipOffset;
                    newHat.BackImage = newHat.LeftBackImage = BackImage;
                    newHat.ClimbImage = newHat.LeftClimbImage = BackImage;
                    newHat.FloorImage = newHat.LeftFloorImage = FloorImage;
                    
                    return newHat;
                }
                catch (Exception e)
                {
                    PeasAPI.Logger.LogError($"Error while creating a hat: {e.StackTrace}");
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
                    newVisor.IdleFrame = newVisor.LeftIdleFrame = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newVisor.ProductId = $"+{Name}";
                    newVisor.Order += 100;
                    newVisor.Free = true;
                    newVisor.name = Name;
                    
                    newVisor.ChipOffset = ChipOffset;
                    newVisor.ClimbFrame = ClimbImage;
                    newVisor.FloorFrame = FloorImage;
                    
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