namespace Platformer.Mechanics {
    public interface IMechanics {
        void Idle();
        void Jump(bool value);
        void Walk(bool value);
        void Run(bool value);
        void Crouch(bool value);
        void Climb(bool value);
    }
}