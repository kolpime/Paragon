using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Paragon.Levels
{
    // Due to my shit graphics card i must use an image no bigger than 128 x 128 and use shorts instead of ints in my indices functions
    // this is because my graphics cars does not support 32 bit indexing.
    // a short has a 16 bit value but has half the maximum and minimum values in it
    // as a result, any image ofer 128x128 causes the indexes to go out the bounds of a short and the game to crash

    //projection matrix is where the clipping plane is set

     
    public class Level1 : GameScreen
    {
        ContentManager Content;
        public struct VertexPositionNormalColored
        {
            public Vector3 Position;
            public Color Color;
            public Vector3 Normal;

            public static int SizeInBytes = 7 * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
              {
                  new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                  new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
                  new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
              };
        }

        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Effect effect;

        VertexPositionNormalColored[] vertices;
        VertexBuffer myVertexBuffer;

        VertexDeclaration myVertexDeclaration;
        short[] indices;
        IndexBuffer myIndexBuffer;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        private int terrainWidth;
        private int terrainHeight;
        private float[,] heightData;

        float angle = 0;

        public Level1()
        {
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            device = ScreenManager.GraphicsDevice;
            spriteBatch = ScreenManager.SpriteBatch;
            effect = Content.Load<Effect>("Effects//render");

            Texture2D heightMap = Content.Load<Texture2D>("Terrain//heightmap"); LoadHeightData(heightMap);

            SetUpIndices();
            SetUpVertices();
            SetUpCamera();
            CalculateNormals();

            CopyToBuffers();
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            { }
            
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Delete))
                angle += 0.05f;
            if (keyState.IsKeyDown(Keys.PageDown))
                angle -= 0.05f;
        }

        public override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            device.RenderState.CullMode = CullMode.None;

            Matrix worldMatrix = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0, terrainHeight / 2.0f) * Matrix.CreateRotationY(angle);
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xWorld"].SetValue(worldMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.1f);


            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.VertexDeclaration = myVertexDeclaration;
                device.Indices = myIndexBuffer;
                device.Vertices[0].SetSource(myVertexBuffer, 0, VertexPositionNormalColored.SizeInBytes);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

                pass.End();
            }
            effect.End();

            base.Draw(gameTime);
        }

        /*************************************************/

        private void SetUpVertices()
        {
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    if (heightData[x, y] < minHeight)
                        minHeight = heightData[x, y];
                    if (heightData[x, y] > maxHeight)
                        maxHeight = heightData[x, y];
                }
            }

            vertices = new VertexPositionNormalColored[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                        vertices[x + y * terrainWidth].Color = Color.Green;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                        vertices[x + y * terrainWidth].Color = Color.Green;
                    else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                        vertices[x + y * terrainWidth].Color = Color.Green;
                    else
                        vertices[x + y * terrainWidth].Color = Color.Green;
                }
            }

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalColored.VertexElements);
        }

        private void SetUpIndices()
        {
            short HEIGHT = (short)terrainHeight;
            short WIDTH = (short)terrainWidth;

            indices = new short[(WIDTH - 1) * (HEIGHT - 1) * 6];
            int count = 0;
            for (int y = 0; y < HEIGHT - 1; y++)
            {
                for (int x = 0; x < WIDTH - 1; x++)
                {
                    indices[count++] = (short)((x + 1) + (y + 1) * WIDTH);
                    indices[count++] = (short)((x + 1) + y * WIDTH);
                    indices[count++] = (short)(x + y * WIDTH);

                    indices[count++] = (short)(x + (y + 1) * WIDTH);
                    indices[count++] = (short)((x + 1) + (y + 1) * WIDTH);
                    indices[count++] = (short)(x + y * WIDTH);
                }
            }

        }

        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                
                    int index1 = indices[i * 3];
                    int index2 = indices[i * 3 + 1];
                    int index3 = indices[i * 3 + 2];

                    Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                    Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                    Vector3 normal = Vector3.Cross(side1, side2);

                    vertices[index1].Normal += normal;
                    vertices[index2].Normal += normal;
                    vertices[index3].Normal += normal;
                
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        private void CopyToBuffers()
        {
            myVertexBuffer = new VertexBuffer(device, vertices.Length * VertexPositionNormalColored.SizeInBytes, BufferUsage.WriteOnly);
            myVertexBuffer.SetData(vertices);

            myIndexBuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.WriteOnly);
            myIndexBuffer.SetData(indices);
        }

        private void SetUpCamera()
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 100, 200), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            //the last variable in this function is the clipping plane i.e. how far it will write out before it clips the image
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f,600.0f);
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;
        }
    }
}
