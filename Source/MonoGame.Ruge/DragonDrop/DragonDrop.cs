/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 
Licensed under MIT (see License.txt)
 
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MonoGame.Ruge.ViewportAdapters;

namespace MonoGame.Ruge.DragonDrop {

    public class DragonDrop<T> : DrawableGameComponent where T : IDragonDropItem {
        private MouseState _oldMouse;
        private MouseState _currentMouse;

        public readonly ViewportAdapter Viewport;

        public T SelectedItem;
        public List<T> DragItems;
        public List<T> MouseItems;


        /// <summary>
        /// Constructor. Uses MonoGame.Extended ViewportAdapter
        /// </summary>
        /// <param name="game"></param>
        /// <param name="sb"></param>
        /// <param name="vp"></param>
        public DragonDrop(Game game, ViewportAdapter vp) : base(game) {
            Viewport = vp;
            SelectedItem = default(T);
            DragItems = new List<T>();
            MouseItems = new List<T>();
        }

        public void Add(T item) {
            DragItems.Add(item);
        }
        public void Remove(T item) { DragItems.Remove(item); }

        public void Clear() {
            SelectedItem = default(T);
            DragItems.Clear();
        }

        private bool Click => _currentMouse.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released;
        private bool UnClick => _currentMouse.LeftButton == ButtonState.Released && _oldMouse.LeftButton == ButtonState.Pressed;
        private bool Drag => _currentMouse.LeftButton == ButtonState.Pressed;

        private Vector2 CurrentMouse {
            get {

                var point = Viewport.PointToScreen(_currentMouse.X, _currentMouse.Y);

                return new Vector2(point.X, point.Y);

            }
        }

        public Vector2 OldMouse {
            get {

                var point = Viewport.PointToScreen(_oldMouse.X, _oldMouse.Y);

                return new Vector2(point.X, point.Y);

            }
        }

        public Vector2 Movement => CurrentMouse - OldMouse;


        private T GetCollusionItem() {

            var items = DragItems.OrderByDescending(z => z.ZIndex).ToList();
            foreach (var item in items) {

                if (item.Contains(CurrentMouse) && !Equals(SelectedItem, item)) return item;

            }
         
            // if it doesn't contain the current mouse, run again to see if it intersects
            foreach (var item in items) {

                if (item.Border.Intersects(SelectedItem.Border) && !Equals(SelectedItem, item)) return item;

            }
            return default(T);

        }

        private T GetMouseHoverItem() {

            var items = DragItems.OrderByDescending(z => z.ZIndex).ToList();

            foreach (var item in items) {

                if (item.Contains(CurrentMouse)) return item;

            }

            return default(T);

        }

        public override void Update(GameTime gameTime) {


            _currentMouse = Mouse.GetState();


            if (SelectedItem != null) {

                if (SelectedItem.IsSelected) {

                    if (Drag) {
                        SelectedItem.Position += Movement;
                        SelectedItem.Update(gameTime);
                    }
                    else if (UnClick) {

                        var collusionItem = GetCollusionItem();

                        if (collusionItem != null) {
                            SelectedItem.OnCollusion(collusionItem);
                            collusionItem.Update(gameTime);
                        }

                        SelectedItem.OnDeselected();
                        SelectedItem.Update(gameTime);

                    }
                }

            }


            foreach (var item in DragItems) {
                item.IsMouseOver = false;
                item.Update(gameTime);
            }

            var hoverItem = GetMouseHoverItem();

            if (hoverItem != null) {

                hoverItem.IsMouseOver = true;

                if (hoverItem.IsDraggable && Click) {
                    SelectedItem = hoverItem;
                    SelectedItem.OnSelected();
                }

                hoverItem.Update(gameTime);

            }


            _oldMouse = _currentMouse;

        }

    }
}
