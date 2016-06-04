/* ©2016 Hathor Gaia 
* http://HathorsLove.com
* 
* Source code licensed under GPL-3
* Assets licensed seperately (see LICENSE.md)
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ruge.CardEngine;
using MonoGame.Ruge.DragonDrop;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace OpenSolitaire.Classic {
    internal class TableClassic : Table {

        public bool IsSetup = false;
        public bool IsSnapAnimating = false;
        
        public Deck DrawPile { get; set; }
        public Stack DiscardPile { get; set; }

        public Slot DrawSlot { get; set; }
        public Slot DiscardSlot { get; set; }

        private MouseState _prevMouseState;
        private SoundEffect _tableAnimationSound;
        private SoundEffect _cardParentSound;
        private SoundEffect _cardPlaySound;
        private SoundEffect _restackSound;
        private SoundEffect _winSound;

        private double _clickTimer;
        private const double _delay = 500;


        public TableClassic(SpriteBatch spriteBatch, DragonDrop<IDragonDropItem> dd, Texture2D cardBack, Texture2D slotTex, int stackOffsetH, int stackOffsetV, List<SoundEffect> soundFx)
            : base(spriteBatch, dd, cardBack, slotTex, stackOffsetH, stackOffsetV) {

            _tableAnimationSound = soundFx[0];
            _cardParentSound = soundFx[1];
            _cardPlaySound = soundFx[2];
            _restackSound = soundFx[3];
            _winSound = soundFx[4];

            // create a fresh card deck
            DrawPile = new Deck(cardBack, slotTex, spriteBatch, stackOffsetH, stackOffsetV) { Type = StackType.Deck };
            DrawPile.FreshDeck();
            
        }


        public void NewGame() {

            foreach (var stack in Stacks) {
                foreach (var card in stack.Cards) DragonDrop.Remove(card);
                stack.Clear();
            }


            DrawPile.FreshDeck();
            DrawPile.Shuffle();
            DrawPile.UpdatePositions();


            foreach (var card in DrawPile.Cards) {
                DragonDrop.Add(card);
            }

        }

        internal void InitializeTable() {
            
            foreach (var card in DrawPile.Cards) {
                DragonDrop.Add(card);
            }

            DrawPile.Shuffle();

            int x = 20;
            int y = 20;

            DrawSlot = new Slot(SlotTex, SpriteBatch) {
                Name = "Draw",
                Position = new Vector2(x, y),
                Stack = DrawPile
            };
            DiscardSlot = new Slot(SlotTex, SpriteBatch) {
                Name = "Discard",
                Position = new Vector2(x * 2 + SlotTex.Width, y),
                Stack = DiscardPile
            };

            DragonDrop.Add(DrawSlot);
            DragonDrop.Add(DiscardSlot);

            DrawPile.Slot = DrawSlot;
            AddStack(DrawPile);

            DiscardPile = AddStack(DiscardSlot, StackType.Discard);
            

            y += SlotTex.Height + y;


            // set up second row of slots
            for (int i = 0; i < 7; i++) {

                // add crunch for these stacks
                var newSlot = new Slot(SlotTex, SpriteBatch) {
                    Position = new Vector2(x + x*i + SlotTex.Width*i, y),
                    Name = "Stack " + i
                };
                
                var newStack = AddStack(newSlot, StackType.Stack, StackMethod.Vertical);
                newStack.CrunchItems = 12;

            }


            y = 20;

            // set up play/score slots
            for (int i = 6; i >= 3; i--) {
                
                var newSlot = new Slot(SlotTex, SpriteBatch) {
                    Position = new Vector2(x + x * i + SlotTex.Width * i, y),
                    Name = "Play " + i
                };

                AddStack(newSlot, StackType.Play);
                
            }

        }


        public new void SetTable() {

            
            foreach (var card in DrawPile.Cards) {
                card.Selected += OnCardSelected;
                card.Collusion += OnCollusion;
                card.Stack = DrawPile;
            }

            int x = 20;
            int y = 20;
            y += SlotTex.Height + y;

            for (var i = 0; i < 7; i++) {

                var pos = new Vector2(x + x * i + SlotTex.Width * i, y);
                var moveCard = DrawPile.DrawCard();
                moveCard.SnapPosition = pos;
                moveCard.IsSnapAnimating = true;
                moveCard.SnapSpeed = 6.0f;
                moveCard.IsDraggable = false;
                Stacks[i+2].AddCard(moveCard);

                for (var j = 1; j < i + 1; j++) {

                    moveCard = DrawPile.DrawCard();
                    moveCard.SnapPosition = new Vector2(pos.X, pos.Y + StackOffsetVertical * j);
                    moveCard.IsSnapAnimating = true;
                    moveCard.SnapSpeed = 6.0f;
                    moveCard.IsDraggable = false;
                    Stacks[i + 2].AddCard(moveCard);

                }


            }

            _tableAnimationSound.Play();

            IsSetup = true;
        }



        private void OnCollusion(object sender, Card.CollusionEvent e) {


            var type = e.Item.GetType();

            if (type == typeof(Card)) {

                Card card, destination;

                var card1 = (Card)sender;
                var card2 = (Card)e.Item;

                //Console.WriteLine("??" + card1.suit.ToString() + card1.rank + " ?? " + card2.suit + card2.rank);

                if (card1.Position != card1.SnapPosition) {
                    card = card1;
                    destination = card2;
                }
                else {
                    destination = card1;
                    card = card2;
                }

                var topCard = destination.Stack.TopCard();

                if (card.IsFaceUp && destination.IsFaceUp && destination == topCard) {

                    Console.WriteLine(card.Suit.ToString() + card.Rank + " -> " + destination.Suit + destination.Rank);

                    
                    if (destination.Stack.Type == StackType.Play && card.Suit == destination.Suit &&
                        card.Rank == destination.Rank + 1) {
                        card.SetParent(destination);
                        _cardPlaySound.Play();
                    }
                    else if (destination.Stack.Type == StackType.Stack && card.Color != destination.Color &&
                        card.Rank == destination.Rank - 1) {
                        card.SetParent(destination);
                        _cardParentSound.Play(.6f, 1f, 1f);
                    }
                    

                    // todo: delete after testing
                    //card.SetParent(destination);

                }

            }
            else if (type == typeof(Slot)) {

                var card = (Card)sender;
                var slot = (Slot)e.Item;


                //Console.WriteLine("(debug) " + card.suit.ToString() + card.rank + " -> " + slot.stack.type);

                if (slot.Stack.Count == 0) {

                    if (slot.Stack.Type == StackType.Play && card.Rank == Rank.A && card.Child == null) {
                        card.MoveToEmptyStack(slot.Stack);
                        _cardPlaySound.Play();
                    }
                    if (slot.Stack.Type == StackType.Stack && card.Rank == Rank.K) {
                        card.MoveToEmptyStack(slot.Stack);
                        _cardParentSound.Play(.6f, 1f, 1f);
                    }

                    Console.WriteLine(card.Suit.ToString() + card.Rank + " -> " + slot.Stack.Type);
                }

            }

        }


        private void OnCardSelected(object sender, EventArgs eventArgs) {

            var card = (Card)sender;

            if (card.IsDraggable) {
                IsSnapAnimating = true;
            }

        }
        

        public new void Update(GameTime gameTime) {

            if (IsSetup) {

                IsSnapAnimating = false;

                foreach (var stack in Stacks) {
                    
                    foreach (var card in stack.Cards) {
                        if (card.IsSnapAnimating) IsSnapAnimating = true;
                    }

                }


                if (!IsSnapAnimating) {


                    foreach (var stack in Stacks) {

                        if (stack.Count > 0) {
                            if (stack.Type == StackType.Stack) {
                                var topCard = stack.TopCard();

                                if (!topCard.IsFaceUp) {
                                    topCard.FlipCard();
                                    topCard.IsDraggable = true;
                                    topCard.SnapSpeed = 25f;
                                    topCard.ZIndex = stack.Count;
                                }
                            }
                            int i = 1;

                            foreach (var card in stack.Cards) {
                                if (!card.IsSelected) card.ZIndex = i;
                                i++;
                            }
                        }
                    }


                    var mouseState = Mouse.GetState();
                    var point = DragonDrop.Viewport.PointToScreen(mouseState.X, mouseState.Y);

                    if (mouseState.LeftButton == ButtonState.Pressed &&
                        _prevMouseState.LeftButton == ButtonState.Released) {

                        if (DrawSlot.Border.Contains(point)) {

                            if (DrawPile.Count > 0) {

                                var card = DrawPile.DrawCard();

                                card.ZIndex = DiscardPile.Count;

                                card.Position = DiscardSlot.Position;
                                card.FlipCard();
                                card.SnapPosition = card.Position;
                                card.IsDraggable = true;
                                DiscardPile.AddCard(card);
                                _cardParentSound.Play(.6f, 1f, 1f);

                            }
                            else if (DrawPile.Count == 0) {

                                while (DiscardPile.Count > 0) {

                                    var disCard = DiscardPile.DrawCard();

                                    disCard.ZIndex = 1;
                                    disCard.FlipCard();
                                    disCard.Position = DrawSlot.Position;
                                    disCard.SnapPosition = DrawSlot.Position;
                                    disCard.IsDraggable = false;

                                    DrawPile.AddCard(disCard);

                                }
                                if (DrawPile.Count > 1) { 
                                    var restackAnimation = DrawPile.TopCard();
                                    restackAnimation.Position += new Vector2(StackOffsetHorizontal * 2,0);
                                    restackAnimation.IsSnapAnimating = true;
                                    restackAnimation.SnapSpeed = 3f;
                                    _restackSound.Play();
                                }
                                else _cardParentSound.Play(.6f, 1f, 1f);
                            }

                        }

                    }
                    
                    _clickTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (mouseState.LeftButton == ButtonState.Pressed &&
                        _prevMouseState.LeftButton == ButtonState.Released) {

                        if (_clickTimer < _delay) {

                            // check for double-click event
                            foreach (var stack in Stacks) {

                                if (stack.Count > 0) { 
                                    if (stack.Type == StackType.Stack || stack.Type == StackType.Discard) {

                                        var topCard = stack.TopCard();

                                        if (topCard.Border.Contains(point) && topCard.Child == null) {

                                            foreach (var playStack in Stacks) {

                                                if (playStack.Type == StackType.Play) {

                                                    if (playStack.Count > 0) {

                                                        var playStackTop = playStack.TopCard();

                                                        if (topCard.Suit == playStackTop.Suit && topCard.Rank == playStackTop.Rank + 1) {
                                                            topCard.SetParent(playStackTop);
                                                            _cardPlaySound.Play();
                                                        }
                                                    }
                                                    else if (topCard.Rank == Rank.A) {
                                                        topCard.MoveToEmptyStack(playStack);
                                                        _cardPlaySound.Play();
                                                    }

                                                }

                                            }

                                            Console.WriteLine("double-click: " + topCard.Suit.ToString() + topCard.Rank);
                                        
                                        }
                                    }
                                }

                            }


                        }
                        _clickTimer = 0;
                    }

                    _prevMouseState = mouseState;
                }
            }

            base.Update(gameTime);
        }

    }
}
