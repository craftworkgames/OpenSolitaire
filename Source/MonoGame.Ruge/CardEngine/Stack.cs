/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)

 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MonoGame.Ruge.DragonDrop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Ruge.CardEngine {
    
    public enum StackMethod {
        Normal,
        Horizontal,
        Vertical,
        Undefined
    }

    public enum StackType {
        Draw,
        Discard,
        Stack,
        Deck,
        Hand,
        Play,
        Undefined
    }

    public class Stack  {

        protected SpriteBatch SpriteBatch;
        public Texture2D CardBack;
        
        public List<Card> Cards = new List<Card>();
        
        public int Count => Cards.Count;

        public StackType Type = StackType.Hand;
        public StackMethod Method = StackMethod.Normal;

        public string Name => Slot.Name;

        protected int StackOffsetHorizontal, StackOffsetVertical;

        public Vector2 Offset {
            get {

                switch (Method) {
                    case StackMethod.Horizontal: return new Vector2(StackOffsetHorizontal, 0);
                    case StackMethod.Vertical:   return new Vector2(0, StackOffsetVertical);
                    default:                     return Vector2.Zero;
                }
            }
        }

        public Slot Slot { get; set; }
        public void Clear() { Cards.Clear(); }

        public int CrunchItems { get; set; } = 0;
        public bool CrunchStacks = false;


        public Stack(Texture2D cardBack, Texture2D slotTex, SpriteBatch spriteBatch, int stackOffsetH, int stackOffsetV) {
            Slot = new Slot(slotTex,spriteBatch) {Stack = this};
            CardBack = cardBack;
            SpriteBatch = spriteBatch;
            StackOffsetHorizontal = stackOffsetH;
            StackOffsetVertical = stackOffsetV;
        }


        public void Shuffle() {

            //wait a few ms to avoid seed collusion
            Thread.Sleep(30);

            var rand = new Random();
            for (int i = Cards.Count - 1; i > 0; i--) {
                int randomIndex = rand.Next(i + 1);
                var tempCard = Cards[i];
                Cards[i] = Cards[randomIndex];
                Cards[randomIndex] = tempCard;
            }
        }

        /// <summary>
        /// just picks the top card on the stack and returns it
        /// </summary>
        /// <returns></returns>
        public Card TopCard() {
            Cards = Cards.OrderBy(z => z.ZIndex).ToList();
            return Cards?.Last();
        }


        private void NukeParents(Card nukeMe) {

            foreach (var card in Cards)
                if (card.Child == nukeMe) card.Child = null;

        }


        public void AddCard(Card card, bool update = false) {
            
            if (card.Stack != null) { 
                card.Stack.Cards.Remove(card);
                card.Stack.NukeParents(card);
            }
            card.Stack = this;
            Cards.Add(card);
            card.ZIndex = Count + 1;
            
            var fixChild = card.Child;

            while (fixChild != null) {

                if (fixChild.Stack != null) {
                    fixChild.Stack.Cards.Remove(fixChild);
                    fixChild.Stack.NukeParents(fixChild);
                }
                
                fixChild.Stack = this;

                Cards.Add(fixChild);

                fixChild = fixChild.Child;

            }


            int i = 0;

            foreach (var fixIndex in Cards) fixIndex.ZIndex = i++;
            
            if (update) UpdatePositions();
        }

        
        public void UpdatePositions() {
            
            int i = 0;
            int numFaceDown = 0;
            
            Cards = Cards.OrderBy(z => z.ZIndex).ToList();
            foreach (var card in Cards) {

                if (!card.IsFaceUp) numFaceDown++;

                var stackOffestX = Offset.X;
                var stackOffestY = Offset.Y;

                CrunchStacks = false;

                // the stack has a lot of items so crunch
                if (CrunchItems > 0 && Cards.Count >= CrunchItems) {

                    if (card.IsFaceUp) {
                            
                        stackOffestX = (stackOffestX > 0) ? stackOffestX - 3 : 0;
                        stackOffestY = (stackOffestY > 0) ? stackOffestY - 3 : 0;
                            
                    }
                    else {
                        stackOffestX = stackOffestX / 2;
                        stackOffestY = stackOffestY / 2;
                    }
                    CrunchStacks = true;
                   
                }


                var newCardX = Slot.Position.X + stackOffestX * i;
                var newCardY = Slot.Position.Y + stackOffestY * i;

                if (card.IsFaceUp && CrunchItems > 0 && Cards.Count >= CrunchItems) {
                    newCardX -= Offset.X * numFaceDown / 2 - Offset.X / 2;
                    newCardY -= Offset.Y * numFaceDown / 2 - Offset.Y / 2;

                    if (numFaceDown == 0) {
                        newCardX -= Offset.X / 2;
                        newCardY -= Offset.Y / 2;
                    }

                }

                card.Position = new Vector2(newCardX, newCardY);
                card.SnapPosition = card.Position;

                i++;

            }

        }



        /// <summary>
        /// just picks the top card on the stack and returns it
        /// </summary>
        /// <returns></returns>
        public Card DrawCard() {

            if (Cards.Count > 0) {

                var topCard = Cards[Cards.Count - 1];
                Cards.RemoveAt(Cards.Count - 1);
                return topCard;

            }
            return null;
        }


        #region MonoGame

        public void Update(GameTime gameTime) {
            Slot.Update(gameTime);

            if (CrunchStacks) {
                foreach (var card in Cards) card.Update(gameTime);
                if (Cards.Count < CrunchItems) UpdatePositions();
            }

        }
        
        public void Draw(GameTime gameTime) {
            Slot.Draw(gameTime);
        }

        #endregion
        

        public void Debug() {

            Console.WriteLine("========");
            Console.WriteLine(Name);

            if (Cards.Count > 0) {

                Card top = TopCard(); 
                string strFaceUp = top.IsFaceUp ? "face up" : "face down";
                Console.WriteLine("top " + "z" + top.ZIndex.ToString("00") + ": " + top.Rank + " of " + top.Suit + " (" + strFaceUp + ")");


                foreach (var card in Cards) {

                    strFaceUp = (card.IsFaceUp ? "face up" : "face down");
                    Console.Write("z" + card.ZIndex.ToString("00") + ": " + card.Rank + " of " + card.Suit + " (" + strFaceUp + ")");
                    Console.Write(" - " + card.Stack.Name);
                    
                    if (card.Child != null) {
                        strFaceUp = (card.Child.IsFaceUp ? "face up" : "face down");
                        Console.Write(" -> z" + card.Child.ZIndex.ToString("00") + ": " +
                        card.Child.Rank + " of " + card.Child.Suit + " (" + strFaceUp + ")");
                    }
                    
                    Console.WriteLine();
                }
            }
            else { Console.WriteLine("(empty stack)"); }

        }
    }

}
