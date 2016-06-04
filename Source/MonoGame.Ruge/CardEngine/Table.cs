/* 
© 2016 The Ruge Project (http://ruge.metasmug.com/) 

Licensed under MIT (see License.txt)

 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ruge.DragonDrop;

namespace MonoGame.Ruge.CardEngine {

    public class Table {

        // Z-Index constants
        protected const int OnTop = 1000;
        
        protected int StackOffsetHorizontal, StackOffsetVertical;
        protected Texture2D CardBack, SlotTex;
        protected SpriteBatch SpriteBatch;
        protected DragonDrop<IDragonDropItem> DragonDrop;
        
        public List<Stack> Stacks = new List<Stack>();

        public Table(SpriteBatch spriteBatch, DragonDrop<IDragonDropItem> dragonDrop, Texture2D cardBack, Texture2D slotTex, int stackOffsetH, int stackOffsetV) {
            SpriteBatch = spriteBatch;
            DragonDrop = dragonDrop;
            StackOffsetHorizontal = stackOffsetH;
            StackOffsetVertical = stackOffsetV;
            CardBack = cardBack;
            SlotTex = slotTex;
        }


        public Stack AddStack(Slot slot, StackType type = StackType.Undefined, StackMethod stackMethod = StackMethod.Normal) {

            var stack = new Stack(CardBack, SlotTex, SpriteBatch, StackOffsetHorizontal, StackOffsetVertical) {
                Slot = slot,
                Method = stackMethod,
                Type = type
            };

            slot.Stack = stack;

            Stacks.Add(stack);

            DragonDrop.Add(slot);

            return stack;

        }

        public void AddStack(Stack stack) {

            foreach (var card in stack.Cards) card.Stack = stack;

            stack.UpdatePositions();
            Stacks.Add(stack);
            
        }
        

 //       public void MoveStack(Card card, Stack newStack) {
            
//            newStack.addCard(card);

//        }

        /// <summary>
        /// I recommend only using this for debugging purposes, this is not an ideal way to do things
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Slot GetSlotByName(string name) {

            Slot slot = null;

            foreach (var stack in Stacks) {

                if (stack.Name == name) slot = stack.Slot;

            }

            return slot;

        }


        /// <summary>
        /// I recommend only using this for debugging purposes, this is not an ideal way to do things
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Stack GetStackByName(string name) {

            Stack foundStack = null;

            foreach (var stack in Stacks) {

                if (stack.Name == name) foundStack = stack;

            }

            return foundStack;

        }


        /// <summary>
        /// override this to set up your table
        /// </summary>
        public void SetTable() { }

        /// <summary>
        /// override this to clear the table
        /// </summary>
        public void Clear() { }


        public void Update(GameTime gameTime) {
            foreach (var stack in Stacks) stack.Update(gameTime);

            // fixes the z-ordering stuff
            var items = DragonDrop.DragItems.OrderBy(z => z.ZIndex).ToList();
            foreach (var item in items) {
                var type = item.GetType();
                if (type == typeof(Card)) item.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime) {
            
            foreach (var stack in Stacks) stack.Draw(gameTime);
            
            // fixes the z-ordering stuff
            var items = DragonDrop.DragItems.OrderBy(z => z.ZIndex).ToList();
            foreach (var item in items) {
                var type = item.GetType();
                if (type == typeof(Card)) item.Draw(gameTime);
            }

        }


    }
    

}
