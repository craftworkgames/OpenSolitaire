/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)

 */

using System;
using MonoGame.Ruge.DragonDrop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Ruge.CardEngine {

    public class Deck : Stack {

        public Deck(Texture2D cardBack, Texture2D slotTex, SpriteBatch spriteBatch, int stackOffsetH, int stackOffsetV) 
            : base(cardBack, slotTex, spriteBatch, stackOffsetH, stackOffsetV) {

            Type = StackType.Deck;

        }

        /// <summary>
        /// populate your deck with a typical set of cards
        /// </summary>
        public void FreshDeck() {

            Cards.Clear();

            foreach (Suit mySuit in Enum.GetValues(typeof(Suit))) {

                foreach (Rank myRank in Enum.GetValues(typeof(Rank))) {

                    Cards.Add(new Card(myRank, mySuit, CardBack, SpriteBatch));

                }

            }

        }

        /// <summary>
        /// makes a smaller random deck for testing
        /// </summary>
        /// <param name="numCards"></param>
        public void TestDeck(int numCards) {

            Cards.Clear();

            var subDeck = new Deck(CardBack, Slot.Texture, SpriteBatch, StackOffsetHorizontal, StackOffsetVertical);
            subDeck.FreshDeck();
            subDeck.Shuffle();

            if (numCards <= subDeck.Count) {

                for (int i = 0; i < numCards; i++) {
                    Cards.Add(subDeck.DrawCard());
                }

            }

            subDeck = null;

        }


    }
}
