using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Paragon.LevelData
{
    class Player
    {
        public Vector3 position = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;
        public float rotation = 0.0f;
        public Model myModel;
        public int Lives = 0;

        public bool isAlive;

        public Player(Model myModel, Vector3 position, float rotation, int lives)
        {
            this.myModel = myModel;
            this.position = position;
            this.rotation = rotation;
            isAlive = true;
            this.Lives = lives;
        }


        public void DrawPlayer(float aspectRatio, Vector3 cameraPosition)
        {
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateRotationZ(rotation)
                        * Matrix.CreateTranslation(position);
                    effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                        aspectRatio, 1.0f, 10000.0f);
                }
                mesh.Draw();
            }
        }
    }
}
