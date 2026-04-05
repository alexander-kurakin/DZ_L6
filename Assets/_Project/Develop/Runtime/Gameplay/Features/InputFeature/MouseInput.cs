using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class MouseInput : IMouseInputService
    {
        public bool IsEnabled { get; set; } = true;

        public float HorizontalDelta
        {
            get
            {
                if (IsEnabled == false)
                    return 0f;

                return Input.GetAxisRaw("Mouse X");
            }
        }
    }
}