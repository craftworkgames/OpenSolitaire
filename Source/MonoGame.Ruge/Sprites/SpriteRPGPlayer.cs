/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)
*/

using Microsoft.Xna.Framework;

namespace MonoGame.Ruge.Sprites {

    public class SpriteRpgPlayer {
        
        private int _cols, _width, _height;
        private SpriteAnimator _animateLeft, _animateRight, _animateUp, _animateDown;
        public Rectangle Rect;
        public Vector2 Position, Origin;
        public int Index;

        // this code assumes you're using a 3x4 or 4x4 type of grid layout
        // common with RPG Maker character tile sets.  
        
        public SpriteRpgPlayer(int cols, int width, int height, Vector2 offset, int playerNum = 0) {

            _cols = cols;
            _width = width;
            _height = height;
            Index = playerNum;

            Vector2 idle;
            

            SpriteGrid grid = new SpriteGrid(4, cols, width, height, offset);



            idle = new Vector2(cols - 1, 0);
            _animateDown = new SpriteAnimator(idle, grid);
            idle = new Vector2(cols - 1, 1);
            _animateLeft = new SpriteAnimator(idle, grid);
            idle = new Vector2(cols - 1, 2);
            _animateRight = new SpriteAnimator(idle, grid);
            idle = new Vector2(cols - 1, 3);
            _animateUp = new SpriteAnimator(idle, grid);


            for (int i = 0; i < cols; i++) {

                _animateDown.Add(new Vector2(i, 0));
                _animateLeft.Add(new Vector2(i, 1));
                _animateRight.Add(new Vector2(i, 2));
                _animateUp.Add(new Vector2(i, 3));

            }

            Rect = grid.GetRectangle(new Vector2(cols - 1, 0));

            Origin = new Vector2(width / 2.0f, height / 2.0f);
            
        }

        public SpriteRpgPlayer(int cols, int width, int height) : this(cols, width, height, Vector2.Zero) { }
        public SpriteRpgPlayer(int cols, int width, int height, int playerNum) : this(cols, width, height, Vector2.Zero, playerNum) { }

        public void MoveDown()  { Rect = _animateDown.Play(); }
        public void MoveUp()    { Rect = _animateUp.Play(); }
        public void MoveLeft()  { Rect = _animateLeft.Play(); }
        public void MoveRight() { Rect = _animateRight.Play(); }

    }

}
