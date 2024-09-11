namespace Platformer.Mechanics {
    public interface IMechanics {
        void Jump();
        void Idle();
        void Walk();
        void Run();
        void Crouch();
        void Climb();
    }
}