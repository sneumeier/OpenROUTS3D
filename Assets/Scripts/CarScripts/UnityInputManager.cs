// GENERATED AUTOMATICALLY FROM 'Assets/Resources/InputManager.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @UnityInputManager : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @UnityInputManager()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputManager"",
    ""maps"": [
        {
            ""name"": ""CarCamera"",
            ""id"": ""65e15b52-3998-4388-9538-57487bd0acbb"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""PassThrough"",
                    ""id"": ""ad63e97a-c5ce-4c2f-a1a9-1ed6aa99eb13"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2c82960c-e047-4500-b061-aa1adb60698e"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=30,y=30)"",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b41204bb-6492-402d-a10b-a3079a524a7d"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""CarInputs"",
            ""id"": ""dc318a2c-4b7a-4ea5-ab62-b99a1d66754f"",
            ""actions"": [
                {
                    ""name"": ""Drive"",
                    ""type"": ""Value"",
                    ""id"": ""d2e5b05a-f3fa-4660-b52e-0ad57e8fc655"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Reverse"",
                    ""type"": ""Button"",
                    ""id"": ""54bd2671-9fb2-4ec9-bb92-436071063a13"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Handbrake"",
                    ""type"": ""Button"",
                    ""id"": ""091e0496-16cf-4291-865d-4d88f1e86b65"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Respawn"",
                    ""type"": ""Button"",
                    ""id"": ""4eaa52a1-71d2-45bd-9e60-32eab8fb4d9e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""bb02dce0-cf04-46d5-a5d8-881e24428138"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Switch Lights"",
                    ""type"": ""Button"",
                    ""id"": ""4d28d050-057a-4d28-a234-601474f23b3c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Turn Signal Left"",
                    ""type"": ""Button"",
                    ""id"": ""3bc95c84-80f6-4b3f-8d0c-7c163ab086f9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Turn Signal Right"",
                    ""type"": ""Button"",
                    ""id"": ""2d0cc19c-8f25-412e-8df7-4f82384bd917"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""66638055-d0e4-4c07-9362-548b9aa67a37"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""310e83c5-5fc6-4918-8efb-fd0ec132a6db"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""70b4dc16-5ac3-4bb2-8447-438a6f06ad8a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""1733c199-8990-4801-97b0-ebd440980431"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8a58c27d-b5e9-4093-90e7-a053123ea643"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""f6efad61-e9bd-468c-9039-8ad9a00b906f"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""2f6d2c4b-120f-4f11-961e-b8939e75e38f"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""fb3d6b77-2799-4d74-b394-8ebb966a0a02"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""f4e6e120-ade9-425c-aaa6-be109317d825"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b8b815f5-dcbb-414c-bb5c-d22dcf1ae561"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drive"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""536df5b9-19e8-4084-ab61-1cc6d71e3c34"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reverse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""32edad45-9bfc-4308-8b39-b959746e1c1a"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reverse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cb3d1222-60e3-48d9-96e1-ad80cd8232d1"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Handbrake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""91d11cf1-5585-4868-b40c-8c93636e82ca"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Handbrake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cdaf37f6-4dbd-470a-88f8-359ecf1db647"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Respawn"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f2612d4b-a4d8-4b95-829a-a8c15382d259"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Respawn"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b3416d72-59f6-482e-a356-832407dfe726"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68d2ecfd-15c7-4686-9e3f-f8c8a52a2c13"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d637b27d-5ce3-4017-9166-1f8201f26c57"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Switch Lights"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""38271b36-8967-42db-b264-ae5c05a862fc"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Switch Lights"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""23afac73-04b3-444e-bf5e-24cca7e035a1"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn Signal Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c5d66af3-fa25-496f-8fd8-959fe9ca6039"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn Signal Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2de083ee-84d5-4e90-8d00-40d8128a125d"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn Signal Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""487e8bd0-2482-4dbe-94c5-310d6f2066c3"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Turn Signal Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""e3a09607-2783-4229-936e-7b22bab4c278"",
            ""actions"": [
                {
                    ""name"": ""Scene"",
                    ""type"": ""Button"",
                    ""id"": ""5686bebc-814d-4faf-8183-d28411f52d4f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f5220343-dc43-47d4-b8cf-d22a3f6b520c"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scene"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // CarCamera
        m_CarCamera = asset.FindActionMap("CarCamera", throwIfNotFound: true);
        m_CarCamera_Look = m_CarCamera.FindAction("Look", throwIfNotFound: true);
        // CarInputs
        m_CarInputs = asset.FindActionMap("CarInputs", throwIfNotFound: true);
        m_CarInputs_Drive = m_CarInputs.FindAction("Drive", throwIfNotFound: true);
        m_CarInputs_Reverse = m_CarInputs.FindAction("Reverse", throwIfNotFound: true);
        m_CarInputs_Handbrake = m_CarInputs.FindAction("Handbrake", throwIfNotFound: true);
        m_CarInputs_Respawn = m_CarInputs.FindAction("Respawn", throwIfNotFound: true);
        m_CarInputs_Pause = m_CarInputs.FindAction("Pause", throwIfNotFound: true);
        m_CarInputs_SwitchLights = m_CarInputs.FindAction("Switch Lights", throwIfNotFound: true);
        m_CarInputs_TurnSignalLeft = m_CarInputs.FindAction("Turn Signal Left", throwIfNotFound: true);
        m_CarInputs_TurnSignalRight = m_CarInputs.FindAction("Turn Signal Right", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Scene = m_UI.FindAction("Scene", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // CarCamera
    private readonly InputActionMap m_CarCamera;
    private ICarCameraActions m_CarCameraActionsCallbackInterface;
    private readonly InputAction m_CarCamera_Look;
    public struct CarCameraActions
    {
        private @UnityInputManager m_Wrapper;
        public CarCameraActions(@UnityInputManager wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_CarCamera_Look;
        public InputActionMap Get() { return m_Wrapper.m_CarCamera; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CarCameraActions set) { return set.Get(); }
        public void SetCallbacks(ICarCameraActions instance)
        {
            if (m_Wrapper.m_CarCameraActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_CarCameraActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_CarCameraActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_CarCameraActionsCallbackInterface.OnLook;
            }
            m_Wrapper.m_CarCameraActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
            }
        }
    }
    public CarCameraActions @CarCamera => new CarCameraActions(this);

    // CarInputs
    private readonly InputActionMap m_CarInputs;
    private ICarInputsActions m_CarInputsActionsCallbackInterface;
    private readonly InputAction m_CarInputs_Drive;
    private readonly InputAction m_CarInputs_Reverse;
    private readonly InputAction m_CarInputs_Handbrake;
    private readonly InputAction m_CarInputs_Respawn;
    private readonly InputAction m_CarInputs_Pause;
    private readonly InputAction m_CarInputs_SwitchLights;
    private readonly InputAction m_CarInputs_TurnSignalLeft;
    private readonly InputAction m_CarInputs_TurnSignalRight;
    public struct CarInputsActions
    {
        private @UnityInputManager m_Wrapper;
        public CarInputsActions(@UnityInputManager wrapper) { m_Wrapper = wrapper; }
        public InputAction @Drive => m_Wrapper.m_CarInputs_Drive;
        public InputAction @Reverse => m_Wrapper.m_CarInputs_Reverse;
        public InputAction @Handbrake => m_Wrapper.m_CarInputs_Handbrake;
        public InputAction @Respawn => m_Wrapper.m_CarInputs_Respawn;
        public InputAction @Pause => m_Wrapper.m_CarInputs_Pause;
        public InputAction @SwitchLights => m_Wrapper.m_CarInputs_SwitchLights;
        public InputAction @TurnSignalLeft => m_Wrapper.m_CarInputs_TurnSignalLeft;
        public InputAction @TurnSignalRight => m_Wrapper.m_CarInputs_TurnSignalRight;
        public InputActionMap Get() { return m_Wrapper.m_CarInputs; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CarInputsActions set) { return set.Get(); }
        public void SetCallbacks(ICarInputsActions instance)
        {
            if (m_Wrapper.m_CarInputsActionsCallbackInterface != null)
            {
                @Drive.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnDrive;
                @Drive.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnDrive;
                @Drive.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnDrive;
                @Reverse.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnReverse;
                @Reverse.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnReverse;
                @Reverse.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnReverse;
                @Handbrake.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnHandbrake;
                @Handbrake.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnHandbrake;
                @Handbrake.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnHandbrake;
                @Respawn.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnRespawn;
                @Respawn.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnRespawn;
                @Respawn.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnRespawn;
                @Pause.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnPause;
                @SwitchLights.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnSwitchLights;
                @SwitchLights.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnSwitchLights;
                @SwitchLights.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnSwitchLights;
                @TurnSignalLeft.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalLeft;
                @TurnSignalLeft.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalLeft;
                @TurnSignalLeft.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalLeft;
                @TurnSignalRight.started -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalRight;
                @TurnSignalRight.performed -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalRight;
                @TurnSignalRight.canceled -= m_Wrapper.m_CarInputsActionsCallbackInterface.OnTurnSignalRight;
            }
            m_Wrapper.m_CarInputsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Drive.started += instance.OnDrive;
                @Drive.performed += instance.OnDrive;
                @Drive.canceled += instance.OnDrive;
                @Reverse.started += instance.OnReverse;
                @Reverse.performed += instance.OnReverse;
                @Reverse.canceled += instance.OnReverse;
                @Handbrake.started += instance.OnHandbrake;
                @Handbrake.performed += instance.OnHandbrake;
                @Handbrake.canceled += instance.OnHandbrake;
                @Respawn.started += instance.OnRespawn;
                @Respawn.performed += instance.OnRespawn;
                @Respawn.canceled += instance.OnRespawn;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @SwitchLights.started += instance.OnSwitchLights;
                @SwitchLights.performed += instance.OnSwitchLights;
                @SwitchLights.canceled += instance.OnSwitchLights;
                @TurnSignalLeft.started += instance.OnTurnSignalLeft;
                @TurnSignalLeft.performed += instance.OnTurnSignalLeft;
                @TurnSignalLeft.canceled += instance.OnTurnSignalLeft;
                @TurnSignalRight.started += instance.OnTurnSignalRight;
                @TurnSignalRight.performed += instance.OnTurnSignalRight;
                @TurnSignalRight.canceled += instance.OnTurnSignalRight;
            }
        }
    }
    public CarInputsActions @CarInputs => new CarInputsActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Scene;
    public struct UIActions
    {
        private @UnityInputManager m_Wrapper;
        public UIActions(@UnityInputManager wrapper) { m_Wrapper = wrapper; }
        public InputAction @Scene => m_Wrapper.m_UI_Scene;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Scene.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScene;
                @Scene.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScene;
                @Scene.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScene;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Scene.started += instance.OnScene;
                @Scene.performed += instance.OnScene;
                @Scene.canceled += instance.OnScene;
            }
        }
    }
    public UIActions @UI => new UIActions(this);
    public interface ICarCameraActions
    {
        void OnLook(InputAction.CallbackContext context);
    }
    public interface ICarInputsActions
    {
        void OnDrive(InputAction.CallbackContext context);
        void OnReverse(InputAction.CallbackContext context);
        void OnHandbrake(InputAction.CallbackContext context);
        void OnRespawn(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnSwitchLights(InputAction.CallbackContext context);
        void OnTurnSignalLeft(InputAction.CallbackContext context);
        void OnTurnSignalRight(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnScene(InputAction.CallbackContext context);
    }
}
