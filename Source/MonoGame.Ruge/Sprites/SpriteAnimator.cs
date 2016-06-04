/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MonoGame.Ruge.Sprites {
    internal class SpriteAnimator {
        private SpriteGrid _spriteGrid;
        private List<Vector2> _queue;
        private Vector2 _idle;
        private int _playIndex = 0;
        private bool _loop;

        public SpriteAnimator(Vector2 idle, SpriteGrid spriteGrid, bool loop = true) {

            _idle = idle;
            _spriteGrid = spriteGrid;
            _loop = loop;
            _queue = new List<Vector2>();

        }

        public void Add(Vector2 vector) { _queue.Add(vector); }

        public void Clear() { _queue.Clear();  }


        public Rectangle Play() {

            Rectangle rect = _spriteGrid.GetRectangle(_idle);

            if (_queue.Count > 0) { 

                // check if it is out of range
                if (_playIndex == _queue.Count) {

                    if (!_loop) { 
                        _queue.Clear();
                        return rect;
                    }
                    else {
                        _playIndex = 0;
                    }

                }
            
                Vector2 current = _queue[_playIndex];
                rect = _spriteGrid.GetRectangle(current);
            
                _playIndex++;
            }

            return rect;

        }

        public Rectangle Stop() {

            return _spriteGrid.GetRectangle(_idle);

        }
        
    }

}
