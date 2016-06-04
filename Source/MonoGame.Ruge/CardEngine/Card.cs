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

    public enum Suit {

        Clubs,
        Hearts,
        Diamonds,
        Spades

    };

    public enum CardColor {

        Red,
        Black

    }

    public enum Rank {
        A,
        _2,
        _3,
        _4,
        _5,
        _6,
        _7,
        _8,
        _9,
        _10,
        J,
        Q,
        K
    }

    public class Card : IDragonDropItem {

        // Z-Index constants
        protected const int OnTop = 1000;

        private readonly SpriteBatch _spriteBatch;

        private Vector2 _position;
        public Vector2 Position {
            get { return _position; }
            set {
                
                _position = value;
                
                if (Child != null) {
                    
                    Vector2 pos = new Vector2(_position.X + Stack.Offset.X, _position.Y + Stack.Offset.Y);

                    Child.Position = pos;
                    Child.SnapPosition = pos;
                    Child.ZIndex = ZIndex + 1;
                    
                }

            }
        }
        
        public Vector2 SnapPosition { get; set; }
        public Card Child { get; set; } = null;

        public Rectangle Border => new Rectangle((int) Position.X, (int) Position.Y, Texture.Width, Texture.Height);

        protected Texture2D CardBack, texture;

        public Texture2D Texture => IsFaceUp ? texture : CardBack;
        public void SetTexture(Texture2D newTexture) => texture = newTexture;

        public Stack Stack { get; set; }
        public int ZIndex { get; set; } = 1;

        public bool IsSnapAnimating = false;
        public bool Snap = true;
        public float SnapSpeed = 25f;

        public CardColor Color {
            get {
                if (Suit.Equals(Suit.Hearts) || Suit.Equals(Suit.Diamonds)) return CardColor.Red;
                else return CardColor.Black;
            }
        }

        public Rank Rank;
        public Suit Suit;

        public bool IsFaceUp = false;

        public Card(Rank rank, Suit suit, Texture2D cardBack, SpriteBatch spriteBatch) {

            _spriteBatch = spriteBatch;
            Rank = rank;
            Suit = suit;
            CardBack = cardBack;

        }


        public void FlipCard() {
            IsFaceUp = !IsFaceUp;
        }


        #region DragonDrop Stuff

        public bool IsSelected { get; set; } = false;
        
        public bool IsMouseOver { get; set; }

        public bool IsDraggable { get; set; } = false;
        public bool Contains(Vector2 pointToCheck) {
            var mouse = new Point((int)pointToCheck.X, (int)pointToCheck.Y);
            return Border.Contains(mouse);
        }

        #endregion

        #region MonoGame
        

        public void Update(GameTime gameTime) {
            

            if (IsSelected) {
                var fixChild = Child;

                while (fixChild != null) {
                    fixChild.ZIndex += OnTop;
                    fixChild = fixChild.Child;
                }
            }
            
            if (IsSnapAnimating) {

                IsSnapAnimating = !SnapAnimation();

            }

        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Draw(Texture, Position, Microsoft.Xna.Framework.Color.White);
        }

        #endregion

        public void MoveToEmptyStack(Stack newStack) {

            if (newStack.Count == 0) newStack.AddCard(this, true);

        }



        public void SetParent(Card parent) {
            
            parent.Child = this;
            parent.Stack.AddCard(this, true);

        }



        /// <summary>
        /// Animation for returning the card to its original position if it can't find a new place to snap to
        /// </summary>
        /// <returns>returns true if the card is back in its original position; otherwise it increments the animation</returns>
        private bool SnapAnimation() {

            var backAtOrigin = false;

            var pos = Position;

            float distance = (float)Math.Sqrt(Math.Pow(SnapPosition.X - pos.X, 2) + (float)Math.Pow(SnapPosition.Y - pos.Y, 2));
            float directionX = (SnapPosition.X - pos.X) / distance;
            float directionY = (SnapPosition.Y - pos.Y) / distance;

            pos.X += directionX * SnapSpeed;
            pos.Y += directionY * SnapSpeed;


            if (Math.Sqrt(Math.Pow(pos.X - Position.X, 2) + Math.Pow(pos.Y - Position.Y, 2)) >= distance) {

                Position = SnapPosition;

                backAtOrigin = true;

                ZIndex -= OnTop;

                if (Stack.CrunchStacks) Stack.UpdatePositions();

            }
            else Position = pos;

            return backAtOrigin;

        }


        #region events

        public event EventHandler Selected;

        public void OnSelected() {
            
//            Console.WriteLine("mouse: " + suit + "-" + rank + " - selected");

            if (IsDraggable) {
                IsSelected = true;
            }
            ZIndex += OnTop;

            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Deselected;

        public void OnDeselected() {

//            Console.WriteLine("mouse: " + suit + "-" + rank + " - deselected");

            IsSelected = false;

            if (Position != SnapPosition) IsSnapAnimating = true;

            Deselected?.Invoke(this, EventArgs.Empty);

        }


        public event EventHandler<CollusionEvent> Collusion;

        public void OnCollusion(IDragonDropItem item) {

            var e = new CollusionEvent {Item = item};

            Collusion?.Invoke(this, e);

        }

        public class CollusionEvent : EventArgs {

            public IDragonDropItem Item { get; set; }

        }

#endregion

    }
}
