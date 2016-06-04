/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)

 */

using System;
using System.Collections.Generic;
using MonoGame.Ruge.DragonDrop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Ruge.CardEngine {
    
    public class Slot : IDragonDropItem {
        
        public Vector2 Position { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsMouseOver { get; set; }

        public Rectangle Border => new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public string Name { get; set; } = "Slot";

        public bool IsDraggable { get; set; }
        public int ZIndex { get; set; } = -1;
        public Texture2D Texture { get; set; }

        public Stack Stack { get; set; }

        private readonly SpriteBatch _spriteBatch;

        public Slot(Texture2D slotTex, SpriteBatch spriteBatch) {

            _spriteBatch = spriteBatch;
            Texture = slotTex;
            
        }

        public bool Contains(Vector2 pointToCheck) {
            var mouse = new Point((int)pointToCheck.X, (int)pointToCheck.Y);
            return Border.Contains(mouse);
        }

        public void OnSelected() { }
        public void OnDeselected() {}
        public void OnCollusion(IDragonDropItem item) { }

        private bool _lastMouseOver = false;

        public void Update(GameTime gameTime) {


            if (_lastMouseOver != IsMouseOver) {

                string mouseOver = IsMouseOver ? "enter" : "exit";
  //              Console.WriteLine("mouse: " + name + "-" + mouseOver);

            }
        
            _lastMouseOver = IsMouseOver;

        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Draw(Texture, Position, Color.White);
        }


    }
}
