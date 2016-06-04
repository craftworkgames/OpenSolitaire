using Microsoft.Xna.Framework;

namespace MonoGame.Ruge.CardEngine {
    public class StackCrunch {

        // define the number of items in a stack before attempting to crunch
        public int CrunchItemMin { get; set; } = 10;
        public int FaceDownCrunch { get; set; }
        public int FaceUpCrunch { get; set; }

        public StackCrunch(int stackOffset) {

            FaceDownCrunch = stackOffset/3;
            FaceUpCrunch = stackOffset/2;

        }

    }
}
