﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InterfaceManager : MonoBehaviour
{
	//controls all the UI in the game
	[HideInInspector] public GlobalRefManager globalRefManager;
	public Image backgroundBlur;
	public UserInterface activeUserInterface, errorModal;
	public GameObject notificationInterfacePrefab, notificationHolder;
	public int notificationPersistUptimeSeconds;
	public bool userIsHoveredOnInterfaceElement;
	public UserInterface worldPosHoverHUD;
	public bool hoverHUDEnabled;
	public Vector3 hoverHudOffset;
	public List<UserInterface> allUserInterfaces;
	public List<SO_NotificationType> notificationTypes;
	[HideInInspector] public Queue<UserInterface> activeNotificationQueue;
	//[HideInInspector] public Stack<UserInterface> pastNotificationsStack;

	private void Start()
	{
		InitializeUserInterface();
	}
	private void Update()
	{
		HandlePlayerInputCycle();

	}

	//should be a mess and take care of all the keyboard input in one place so it doesnt get spread around the other managers
	private void HandlePlayerInputCycle()
	{
		if (Input.anyKey)
		{
			if (Input.GetKeyDown(globalRefManager.settingsManager.openHomeMenu))
			{
				SetMajorInterface("Home");
				return;
			}
			if (activeUserInterface != null && Input.GetKeyDown(globalRefManager.settingsManager.openPauseMenu))
			{
				SetMajorInterface("Pause");
				return;
			}
			if (Input.GetKeyDown(globalRefManager.settingsManager.closeAllInterfaces))
			{
				CloseAllInterfaces();
				return;
			}
			if (Input.GetKeyDown(globalRefManager.settingsManager.setModePlayer))
			{
				globalRefManager.baseManager.SetPlayerState(BaseManager.PlayerState.PlayerMode);
			}
			if (Input.GetKeyDown(globalRefManager.settingsManager.setModeEdit))
			{
				globalRefManager.baseManager.SetPlayerState(BaseManager.PlayerState.EditMode);
			}
			if (Input.GetKeyDown(globalRefManager.settingsManager.setModeBuild))
			{
				globalRefManager.baseManager.SetPlayerState(BaseManager.PlayerState.BuildMode);
			}
		}
	}

	// sets all the default or player pref values for what all the interfaces should look like on startup of the main game
	private void InitializeUserInterface()
	{
		SetBackgroundBlur(false);
		globalRefManager.baseManager.gameIsActivelyFrozen = false;
		allUserInterfaces = new List<UserInterface>();
		allUserInterfaces.AddRange((UserInterface[])Resources.FindObjectsOfTypeAll(typeof(UserInterface)));
		foreach (UserInterface userInterface in allUserInterfaces)
		{
			userInterface.interfaceManager = this;
			if(userInterface.interfaceType != UserInterface.InterfaceType.HUD &&
				userInterface.interfaceType != UserInterface.InterfaceType.WorldSpace &&
				userInterface.interfaceType != UserInterface.InterfaceType.Notification)
			userInterface.gameObject.SetActive(false);
		}
		activeNotificationQueue = new Queue<UserInterface>();
	}
	//opens up a fullscreen interface / menu
	public void SetMajorInterface(string UiName)
	{
		UserInterface UI = GetUserInterface(UiName);
		if(activeUserInterface!=null)
			activeUserInterface.gameObject.SetActive(false);
		activeUserInterface = UI;
		activeUserInterface.gameObject.SetActive(true);
		SetInterfaceLanguage(UI);
		SetBackgroundBlur(UI.interfaceType == UserInterface.InterfaceType.FullScreen || UI.interfaceType == UserInterface.InterfaceType.Modal);
		globalRefManager.baseManager.gameIsActivelyFrozen = UI.interfaceType == UserInterface.InterfaceType.FullScreen || UI.interfaceType == UserInterface.InterfaceType.Modal;
	}

	//set worldposition viewer state
	public void SetWorldPositionViewerState(bool enabled, RoomTile rt)
	{
		hoverHUDEnabled = enabled;
		worldPosHoverHUD.gameObject.SetActive(enabled);
		if (enabled)
		{
			worldPosHoverHUD.transform.position = hoverHudOffset + globalRefManager.baseManager.editModePermSelectedRoomTile.transform.position;
			worldPosHoverHUD.interfaceName.text = rt.tileType.tileTypeName;
			worldPosHoverHUD.interfaceDescription.text = rt.tileType.tileInformation;
		}
		else
		{
			userIsHoveredOnInterfaceElement = false;
		}
	}

	//closes the currently hovered tile menu thing
	public void CloseWorldPositionViewer()
	{
		SetWorldPositionViewerState(false, null);
		if (globalRefManager.baseManager.editModePermSelectedRoomTile != null)
			globalRefManager.baseManager.editModePermSelectedRoomTile.roomContainer.SetRoomTint(Color.white);
		globalRefManager.baseManager.editModePermSelectedRoomTile = null;
	}

	//recives the call to delete the currently seleced tile
	public void DeleteCurrentltySelecedTile()
	{
		globalRefManager.baseManager.TryDestroyCurrentlySelectedTile();
	}

	//close the currently open interface
	public void CloseAllInterfaces()
	{
		SetBackgroundBlur(false);
		globalRefManager.baseManager.gameIsActivelyFrozen = false;
		activeUserInterface.gameObject.SetActive(false);
		userIsHoveredOnInterfaceElement = false;
	}

	//finds all the text elements in an interface and translates them
	public void SetInterfaceLanguage(UserInterface ui)
	{
		ui.SetChildrenKeys();
	}
	//finds all the text elements in an interface and translates them
	public void SetInterfaceLanguage(UserInterface ui, SO_NotificationType type)
	{
		ui.GetTranslationKey("notification_title").textBox.text = globalRefManager.langManager.GetTranslation(type.notificationName);
		ui.GetTranslationKey("notification_info").textBox.text = globalRefManager.langManager.GetTranslation(type.notificationDescription);
	}

	//toggles the state of the player hovering over a UI element
	public void SetInterfaceHoverState(bool state)
	{
		userIsHoveredOnInterfaceElement = state;
	}

	//returns the interface with the given ID, or defaults to the error modal
	public UserInterface GetUserInterface(string callbackID)
	{
		foreach (UserInterface userInterface in allUserInterfaces)
		{
			if (userInterface.interfaceCallbackID.ToLower() == callbackID.ToLower())
			{
				return userInterface;
			}
		}
		return errorModal; //defaults to the error modal
	}

	//returns a notification type if it exits
	public SO_NotificationType GetNotificationType(string ID)
	{
		foreach (SO_NotificationType type in notificationTypes)
		{
			if (type.callbackID.ToLower() == ID.ToLower())
				return type;
		}
		return notificationTypes[0]; //defaults to the not found notification, does not show error modal and interrupt tho
	}

	//adds a notification to the end of the stream of notifications to be seen
	public void EnqueueNotification(string _type, string customDescription)
	{
		SO_NotificationType type = GetNotificationType(_type);
		GameObject note = Instantiate(notificationInterfacePrefab);
		note.SetActive(true);
		note.transform.SetParent(notificationHolder.transform);
		UserInterface ui = note.GetComponent<UserInterface>();
		SetInterfaceLanguage(ui, type);
		ui.mainInterfaceIcon.sprite = type.notificationIcon;
		ui.saveNotification = type.shouldBeSaved;
		ui.interfaceManager = this;
		activeNotificationQueue.Enqueue(ui);
		StartCoroutine(ui.DelayToClose(notificationPersistUptimeSeconds));
	}

	//removes the oldest notification
	public void DequeueNotification(UserInterface ui)
	{
		activeNotificationQueue.Dequeue();
		Destroy(ui.gameObject);

		//NOTE notification UI gets destroyed cause useless.. idk what to put here. figure it out when making old notes UI ig
		//if (notification.saveNotification)
		//	pastNotificationsStack.Push(notification);
	}

	//sets the state of the background blur
	public void SetBackgroundBlur(bool state)
	{
		backgroundBlur.enabled = state;
	}
}
