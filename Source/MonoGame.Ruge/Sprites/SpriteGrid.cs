/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Ruge.Sprites {
    internal class SpriteGrid {

        private int _rows, _cols, _width, _height;
        private Vector2 _offset;

        public SpriteGrid(int rows, int cols, int width, int height, Vector2 offset) {

            _rows = rows;
            _cols = cols;
            _width = width;
            _height = height;
            _offset = offset;

        }

        public SpriteGrid(int rows, int cols, int width, int height) : this(rows, cols, width, height, Vector2.Zero) {}

            
        public Rectangle GetRectangle(Vector2 vector) {

            int x = (int)vector.X * _width;
            int y = (int)vector.Y * _height;

            x += (int)_offset.X;
            y += (int)_offset.Y;

            Rectangle rect = new Rectangle(x, y, _width, _height);
            return rect;

        }

        public Rectangle GetRectangle(int col, int row) {

            Vector2 vect = new Vector2(col, row);
            Rectangle rect = GetRectangle(vect);
            return rect;

        }

    }
}
