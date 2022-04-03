using System;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;

namespace ChatboxEdit
{
	[RequireComponent(typeof(MPEventSystemLocator))]
	public class ChatBoxPickup : MonoBehaviour
	{
		// Token: 0x06002E0B RID: 11787 RVA: 0x000B99CC File Offset: 0x000B7BCC
		private void UpdateFade(float deltaTime)
		{
			
			this.fadeTimer -= deltaTime;
			/*if (!this.fadeGroup)
			{
				return;
			}*/
			float alpha;
			if (this.showInput)
			{
				alpha = 1f;
				this.ResetFadeTimer();
			}
			else if (this.fadeTimer < 0f)
			{
				alpha = 0f;
			}
			else if (this.fadeTimer < this.fadeDuration)
			{
				alpha = Mathf.Sqrt(Util.Remap(this.fadeTimer, this.fadeDuration, 0f, 1f, 0f));
			}
			else
			{
				alpha = 1f;
			}
			//this.fadeGroup.alpha = alpha;
		}

		// Token: 0x06002E0C RID: 11788 RVA: 0x000B9A68 File Offset: 0x000B7C68
		private void ResetFadeTimer()
		{
			this.fadeTimer = this.fadeDuration + this.fadeWait;
		}

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06002E0D RID: 11789 RVA: 0x000B9A7D File Offset: 0x000B7C7D
		private bool showKeybindTipOnStart
		{
			get
			{
				return ChatPickup.readOnlyLog.Count == 0;
			}
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x06002E0E RID: 11790 RVA: 0x000B9A8C File Offset: 0x000B7C8C
		// (set) Token: 0x06002E0F RID: 11791 RVA: 0x000B9A94 File Offset: 0x000B7C94
		private bool showInput
		{
			get
			{
				return this._showInput;
			}
			set
			{
				return;
				if (this._showInput != value)
				{
					this._showInput = value;
					this.RebuildChatRects();
					if (this.inputField && this.deactivateInputFieldIfInactive)
					{
						this.inputField.gameObject.SetActive(this._showInput);
					}
					if (this.sendButton)
					{
						this.sendButton.gameObject.SetActive(this._showInput);
					}
					for (int i = 0; i < this.gameplayHiddenGraphics.Length; i++)
					{
						this.gameplayHiddenGraphics[i].enabled = this._showInput;
					}
					if (this._showInput)
					{
						this.FocusInputField();
						return;
					}
					this.UnfocusInputField();
				}
			}
		}

		// Token: 0x06002E10 RID: 11792 RVA: 0x000B9B43 File Offset: 0x000B7D43
		public void SetShowInput(bool value)
		{
			this.showInput = value;
		}

		// Token: 0x06002E11 RID: 11793 RVA: 0x000B9B4C File Offset: 0x000B7D4C
		public void SubmitChat()
		{
			string text = this.inputField.text;
			if (text != "")
			{
				this.inputField.text = "";
				ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList = NetworkUser.readOnlyLocalPlayersList;
				if (readOnlyLocalPlayersList.Count > 0)
				{
					string text2 = text;
					text2 = text2.Replace("\\", "\\\\");
					text2 = text2.Replace("\"", "\\\"");
					RoR2.Console.instance.SubmitCmd(readOnlyLocalPlayersList[0], "say \"" + text2 + "\"", false);
					Debug.Log("Submitting say cmd.");
				}
			}
			Debug.LogFormat("SubmitChat() submittedText={0}", new object[]
			{
				text
			});
			if (this.deselectAfterSubmitChat)
			{
				this.showInput = false;
				return;
			}
			this.FocusInputField();
		}

		// Token: 0x06002E12 RID: 11794 RVA: 0x00004381 File Offset: 0x00002581
		public void OnInputFieldEndEdit()
		{
		}

		// Token: 0x06002E13 RID: 11795 RVA: 0x000B9C0B File Offset: 0x000B7E0B
		private void Awake()
		{
			this.eventSystemLocator = base.GetComponent<MPEventSystemLocator>();
			this.showInput = true;
			this.showInput = false;
			Chat.onChatChanged += this.OnChatChangedHandler;
		}

		// Token: 0x06002E14 RID: 11796 RVA: 0x000B9C38 File Offset: 0x000B7E38
		private void OnDestroy()
		{
			Chat.onChatChanged -= this.OnChatChangedHandler;
		}

		// Token: 0x06002E15 RID: 11797 RVA: 0x000B9C4B File Offset: 0x000B7E4B
		private void Start()
		{
			if (this.showKeybindTipOnStart && !RoR2Application.isInSinglePlayer)
			{
				ChatPickup.AddMessage(Language.GetString("CHAT_KEYBIND_TIP"));
			}
			this.BuildChat();
			this.ScrollToBottom();
			this.inputField.resetOnDeActivation = true;
		}

		// Token: 0x06002E16 RID: 11798 RVA: 0x000B9C83 File Offset: 0x000B7E83
		private void OnEnable()
		{
			this.BuildChat();
			this.ScrollToBottom();
			base.Invoke("ScrollToBottom", 0f);
		}

		// Token: 0x06002E17 RID: 11799 RVA: 0x00004381 File Offset: 0x00002581
		private void OnDisable()
		{
		}

		// Token: 0x06002E18 RID: 11800 RVA: 0x000B9CA1 File Offset: 0x000B7EA1
		private void OnChatChangedHandler()
		{
			this.ResetFadeTimer();
			if (base.enabled)
			{
				this.BuildChat();
				this.ScrollToBottom();
			}
		}

		// Token: 0x06002E19 RID: 11801 RVA: 0x000B9CBD File Offset: 0x000B7EBD
		public void ScrollToBottom()
		{
			this.messagesText.verticalScrollbar.value = 0f;
			this.messagesText.verticalScrollbar.value = 1f;
		}

		// Token: 0x06002E1A RID: 11802 RVA: 0x000B9CEC File Offset: 0x000B7EEC
		private void BuildChat()
		{
			ReadOnlyCollection<string> readOnlyLog = ChatPickup.readOnlyLog;
			string[] array = new string[readOnlyLog.Count];
			readOnlyLog.CopyTo(array, 0);
			this.messagesText.text = string.Join("\n", array);
			this.RebuildChatRects();
		}

		// Token: 0x06002E1B RID: 11803 RVA: 0x000B9D30 File Offset: 0x000B7F30
		private void Update()
		{
			return;
			this.UpdateFade(Time.deltaTime);
			MPEventSystem eventSystem = this.eventSystemLocator.eventSystem;
			GameObject gameObject = eventSystem ? eventSystem.currentSelectedGameObject : null;
			//bool flag = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
			bool flag = false;
			if (!this.showInput && flag && !(ConsoleWindow.instance != null))
			{
				this.showInput = true;
				return;
			}
			if (gameObject == this.inputField.gameObject)
			{
				if (flag)
				{
					if (this.showInput)
					{
						this.SubmitChat();
					}
					else if (!gameObject)
					{
						this.showInput = true;
					}
				}
				if (Input.GetKeyDown(KeyCode.Escape) && true == false)
				{
					this.showInput = false;
					return;
				}
			}
			else
			{
				this.showInput = false;
			}
		}

		// Token: 0x06002E1C RID: 11804 RVA: 0x000B9DF0 File Offset: 0x000B7FF0
		public void RebuildChatRects()
		{
			RectTransform component = this.scrollRect.GetComponent<RectTransform>();
			component.SetParent((this.showInput && this.allowExpandedChatbox) ? this.expandedChatboxRect : this.standardChatboxRect);
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			this.ScrollToBottom();
		}

		// Token: 0x06002E1D RID: 11805 RVA: 0x000B9E48 File Offset: 0x000B8048
		private void FocusInputField()
		{
			MPEventSystem eventSystem = this.eventSystemLocator.eventSystem;
			if (eventSystem)
			{
				eventSystem.SetSelectedGameObject(this.inputField.gameObject);
			}
			this.inputField.ActivateInputField();
			this.inputField.text = "";
		}

		// Token: 0x06002E1E RID: 11806 RVA: 0x000B9E98 File Offset: 0x000B8098
		private void UnfocusInputField()
		{
			MPEventSystem eventSystem = this.eventSystemLocator.eventSystem;
			if (eventSystem && eventSystem.currentSelectedGameObject == this.inputField.gameObject)
			{
				eventSystem.SetSelectedGameObject(null);
			}
			this.inputField.DeactivateInputField(false);
		}

		// Token: 0x04002809 RID: 10249
		[Header("Cached Components")]
		public TMP_InputField messagesText;

		// Token: 0x0400280A RID: 10250
		public TMP_InputField inputField;

		// Token: 0x0400280B RID: 10251
		public Button sendButton;

		// Token: 0x0400280C RID: 10252
		public Graphic[] gameplayHiddenGraphics;

		// Token: 0x0400280D RID: 10253
		public RectTransform standardChatboxRect;

		// Token: 0x0400280E RID: 10254
		public RectTransform expandedChatboxRect;

		// Token: 0x0400280F RID: 10255
		public ScrollRect scrollRect;

		// Token: 0x04002810 RID: 10256
		[Tooltip("The canvas group to use to fade this chat box. Leave empty for no fading behavior.")]
		//public CanvasGroup fadeGroup;

		// Token: 0x04002811 RID: 10257
		[Header("Parameters")]
		public bool allowExpandedChatbox;

		// Token: 0x04002812 RID: 10258
		public bool deselectAfterSubmitChat;

		// Token: 0x04002813 RID: 10259
		public bool deactivateInputFieldIfInactive;

		// Token: 0x04002814 RID: 10260
		public float fadeWait = 99999999f;

		// Token: 0x04002815 RID: 10261
		public float fadeDuration = 5f;

		// Token: 0x04002816 RID: 10262
		private float fadeTimer;

		// Token: 0x04002817 RID: 10263
		private MPEventSystemLocator eventSystemLocator;

		// Token: 0x04002818 RID: 10264
		private bool _showInput;
	}
}
