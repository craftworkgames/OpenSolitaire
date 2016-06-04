namespace MonoGame.Ruge.DragonDrop {

    public class CollusionItem {

        public IDragonDropItem Item;
        public bool UnderMouse { get; set; } = false;


        public CollusionItem(IDragonDropItem item) {

            Item = item;

        }

    }

}
