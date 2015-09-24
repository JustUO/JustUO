#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Server.Commands;
using Server.Items;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps.Compendium
{
	public class CompendiumEditorState
	{
		public string PageName { get; set; }
		public BaseCompendiumPageElement SelectedElement { get; set; }
		public CompendiumPageRenderer RendererToEdit { get; set; }
		public CompendiumPageEditor EditorInstance { get; set; }
		public CompendiumPreviewPageGump PreviewGump { get; set; }
		public ElementListGump ElementListGump { get; set; }
		public int ElementListGumpCurrentPageNumber { get; set; }
		public int PropertiesCurrentPageNumber { get; set; }
		public int ElementToolbarPageNumber { get; set; }
		public PlayerMobile Caller;

		public static void Update(object state)
		{
			var editorState = state as CompendiumEditorState;

			if (editorState != null)
			{
				if (editorState.EditorInstance != null)
				{
					editorState.EditorInstance.Refresh(true);
				}

				if (editorState.ElementListGump != null)
				{
					editorState.ElementListGump.Refresh(true);
				}

				if (editorState.PreviewGump != null)
				{
					editorState.PreviewGump.Refresh(true);
				}
			}
		}

		public void Refresh()
		{
			Timer.DelayCall(Update, this);
		}

		public static void Initialize()
		{
			CommandSystem.Register("EditPage", AccessLevel.Administrator, _OnCommand);
		}

		[Usage(""), Description("Makes a call to your custom gump.")]
		public static void _OnCommand(CommandEventArgs e)
		{
			var caller = e.Mobile;

			if (caller.HasGump(typeof(CompendiumPreviewPageGump)))
			{
				caller.CloseGump(typeof(CompendiumPreviewPageGump));
			}

			if (caller.HasGump(typeof(CompendiumPageEditor)))
			{
				caller.CloseGump(typeof(CompendiumPageEditor));
			}

			if (caller.HasGump(typeof(ElementListGump)))
			{
				caller.CloseGump(typeof(ElementListGump));
			}

			if (e.Arguments.Length <= 0)
			{
				return;
			}

			if (e.Arguments[0].IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				caller.SendMessage("That page name has illegal characters in it.");
				return;
			}

			var state = new CompendiumEditorState
			{
				PageName = e.Arguments[0],
				Caller = (PlayerMobile)caller
			};

			if (Compendium.g_CompendiumRenderers.ContainsKey(state.PageName))
			{
				state.RendererToEdit = (CompendiumPageRenderer)Compendium.g_CompendiumRenderers[state.PageName].Clone();
			}
			else
			{
				state.RendererToEdit = new CompendiumPageRenderer
				{
					Name = state.PageName
				};
			}

			state.RendererToEdit.SelectedElement = null;
			state.EditorInstance = new CompendiumPageEditor(state);
			state.RendererToEdit.State = state;
			state.ElementListGump = new ElementListGump(state);

			state.PreviewGump = new CompendiumPreviewPageGump(state.Caller, state.RendererToEdit);

			state.EditorInstance.Send();
			state.ElementListGump.Send();
			state.PreviewGump.Send();
		}
	}

	public class CompendiumElementRegistrationInfo
	{
		public int Index { get; set; }
		public Type ElementType { get; set; }
		public CompendiumPageEditor.CreateInstanceMethod Method { get; set; }
		public string DisplayName { get; set; }
	}

	public delegate void UpdateString(string s);

	public class ElementProperty
	{
		public enum InputType
		{
			TextEntry,
			Book,
			Checkbox,
			RadioButton,
			Blank,
			Label
		}

		public UpdateString BookCallback { get; set; }
		public string Display { get; set; }
		public Action<GumpEntry, object> Callback { get; set; }
		public string Text { get; set; }
		public InputType ElementInputType { get; set; }
		public bool Checked { get; set; }

		public ElementProperty(
			string display,
			Action<GumpEntry, object> callback,
			string text,
			InputType elementInputType,
			bool isChecked = false,
			UpdateString bookTextCallback = null)
		{
			Display = display;
			Callback = callback;
			Text = text;
			ElementInputType = elementInputType;
			Checked = isChecked;
			BookCallback = bookTextCallback;
		}
	}

	public class CompendiumPageEditor : SuperGump
	{
		private const int GRID_BUTTON_ID = 2511;
		private const int NUMBER_OF_PROPERTIES_PER_PAGE = 8;
		private const int NUMBER_OF_REGISTERED_ELEMENTS_PER_COLUMN = 5;
		private const int NUMBER_OF_REGISTERED_ELEMENTS_PER_PAGE = 10;

		public delegate BaseCompendiumPageElement CreateInstanceMethod();

		private static readonly Dictionary<Type, CompendiumElementRegistrationInfo> g_registeredElements =
			new Dictionary<Type, CompendiumElementRegistrationInfo>();

		private static int g_registeredIndex;

		public static void RegisterElementType(Type elementType, CreateInstanceMethod method, string displayName)
		{
			if (!g_registeredElements.ContainsKey(elementType))
			{
				var info = new CompendiumElementRegistrationInfo
				{
					Index = g_registeredIndex,
					ElementType = elementType,
					Method = method,
					DisplayName = displayName
				};
				g_registeredElements.Add(elementType, info);
				g_registeredIndex++;
			}
			else
			{
				Console.WriteLine("That Compendium Page Element has already been registered! " + elementType);
			}
		}

		public CompendiumEditorState EditorState { get; set; }

		public CompendiumPageEditor(CompendiumEditorState state)
			: base(state.Caller, null)
		{
			state.EditorInstance = this;

			EditorState = state;

			Closable = false;
			Disposable = false;
			Dragable = true;
			Resizable = false;
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			#region Borders and Dividers
			AddImage(1, 0, 2623, 1880); //Content Left Border 1
			AddImage(1, 165, 2623, 1880); //Content Left Border 2
			AddImage(1, 320, 2623, 1880); //Content Left Border 2
			AddImage(261, 0, 2625, 1880); //Content Right Border 1
			AddImage(261, 320, 2625, 1880); //Content Right Border 2
			AddImage(261, 165, 2625, 1880); //Content Right Border 2
			AddImage(6, 512, 2627, 1880); //Content Bottom Border 3
			AddImage(7, 0, 2621, 1880); //Content Top Border 3
			AddBackground(10, 11, 264, 507, 9300); //Background 53

			//Lines and Divisors
			AddImageTiled(256, 35, 1, 454, 2623); //Left Divisor
			AddImageTiled(27, 35, 1, 454, 2623); //Right Divisor

			//AddImageTiled(105, 35, 1, 201, 2623); //Left Divisor
			AddImageTiled(27, 236, 230, 1, 2621); //Bottom Line
			AddImageTiled(27, 489, 230, 1, 2621); //Controls Bottom Border
			AddImageTiled(27, 35, 230, 1, 2621); //Top Line
			AddImageTiled(27, 376, 230, 1, 2621); //Controls Top Border
			#endregion

			AddLabel(30, 14, 47, String.Format("Page Name: {0}", EditorState.RendererToEdit.Name)); //Properties Header

			#region Properties
			if (EditorState.SelectedElement != null)
			{
				var properties = EditorState.SelectedElement.GetElementPropertiesSnapshot();

				if (EditorState.PropertiesCurrentPageNumber > properties.Count / NUMBER_OF_PROPERTIES_PER_PAGE)
				{
					EditorState.PropertiesCurrentPageNumber = 0;
				}
				var startingElementIndex = EditorState.PropertiesCurrentPageNumber * NUMBER_OF_PROPERTIES_PER_PAGE;
				var propertiesStartingY = 39;
				for (var i = startingElementIndex;
					i < startingElementIndex + NUMBER_OF_PROPERTIES_PER_PAGE && i < properties.Count;
					++i)
				{
					AddLabel(35, propertiesStartingY, 47, properties[i].Display);

					var index = i;

					switch (properties[i].ElementInputType)
					{
						case ElementProperty.InputType.Book:
						{
							AddButton(134, propertiesStartingY, 5411, 5411, b => onBookEditTextButtonClick(b, index));
						}
							break;

						case ElementProperty.InputType.RadioButton:
						{
							AddRadio(
								134,
								propertiesStartingY,
								209,
								208,
								properties[i].Checked,
								(r, s) =>
								{
									if (properties[index].Callback != null)
									{
										properties[index].Callback(r, s);
									}
								});
						}
							break;

						case ElementProperty.InputType.Label:
						{
							this.AddHtmlLabel(
								134,
								propertiesStartingY,
								FontHandling.FontSize.Medium,
								false,
								false,
								false,
								Compendium.CONTENT_TEXT_WEB_COLOR,
								properties[i].Text);
						}
							break;

						case ElementProperty.InputType.Blank:
						{ }
							break;

						case ElementProperty.InputType.TextEntry:
						{
							if (properties[i].Callback != null)
							{
								AddBackground(130, propertiesStartingY, 120, 20, 9350);
								AddTextEntry(
									134,
									propertiesStartingY,
									137,
									20,
									0,
									properties[i].Text,
									(e, t) =>
									{
										if (properties[index].Callback != null)
										{
											properties[index].Callback(e, t);
										}
									});
							}
						}
							break;

						case ElementProperty.InputType.Checkbox:
						{
							AddCheck(
								134,
								propertiesStartingY,
								210,
								211,
								properties[i].Checked,
								(r, s) =>
								{
									if (properties[index].Callback != null)
									{
										properties[index].Callback(r, s);
									}
								});
						}
							break;
					}

					propertiesStartingY += 22;
				}

				if (EditorState.PropertiesCurrentPageNumber > 0)
				{
					AddButton(30, 216, 5603, 5607, onPropertyPrevPageButtonClick);
				}

				var totalPages = (properties.Count / NUMBER_OF_PROPERTIES_PER_PAGE) + 1;

				if (EditorState.PropertiesCurrentPageNumber < totalPages - 1)
				{
					AddButton(238, 216, 5601, 5605, onPropertyNextPageButtonClick);
				}

				AddImage(109, 216, 2463, 2171);
				this.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(109, 214),
						Compendium.YELLOW_TEXT_WEB_COLOR,
						false,
						false,
						false,
						(b, p) => onApplyPropertiesButtonClick(b),
						0,
						"  Apply  "));
			}
			#endregion

			#region Bottom Buttons
			AddImage(27, 496, 2463, 2171); //Save Button Background
			AddImage(189, 496, 2463, 2171); //Clone Button Background

			//Bottom Buttons
			this.AddHyperlink(
				new WebColoredHyperLink(
					new Point2D(27, 494),
					Compendium.YELLOW_TEXT_WEB_COLOR,
					false,
					false,
					false,
					(b, p) => onSavePageButtonClick(b),
					0,
					"  Save   "));

			this.AddHyperlink(
				new WebColoredHyperLink(
					new Point2D(189, 494),
					Compendium.YELLOW_TEXT_WEB_COLOR,
					false,
					false,
					false,
					(b, p) => onSaveAsButtonClick(b),
					0,
					" Save As..."));
			#endregion

			#region Image Elements Toolbar
			var startingRegElementIdx = EditorState.ElementToolbarPageNumber * NUMBER_OF_REGISTERED_ELEMENTS_PER_PAGE;
			for (var i = startingRegElementIdx;
				i < g_registeredElements.Count && i < startingRegElementIdx + NUMBER_OF_REGISTERED_ELEMENTS_PER_PAGE;
				++i)
			{
				var x = i - startingRegElementIdx < NUMBER_OF_REGISTERED_ELEMENTS_PER_COLUMN ? 55 : 147;
				var y = i - startingRegElementIdx < NUMBER_OF_REGISTERED_ELEMENTS_PER_COLUMN
					? 385 + ((i - startingRegElementIdx) * 20)
					: 385 + ((i - startingRegElementIdx - NUMBER_OF_REGISTERED_ELEMENTS_PER_COLUMN) * 20);

				AddImage(x, y, 2466, 2171); //TextButton Background
				this.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(x, y),
						Compendium.YELLOW_TEXT_WEB_COLOR,
						false,
						false,
						false,
						onAddElementButtonClick,
						i,
						g_registeredElements.ElementAt(i).Value.DisplayName)); //Item Label
			}

			if (EditorState.ElementToolbarPageNumber > 0)
			{
				AddButton(30, 426, 5603, 5607, onElementPrevPageButtonClick);
			}

			var totalElementPages =
				(int)Math.Ceiling(g_registeredElements.Count / (double)NUMBER_OF_REGISTERED_ELEMENTS_PER_PAGE);

			if (EditorState.ElementToolbarPageNumber < totalElementPages - 1)
			{
				AddButton(239, 426, 5601, 5605, onElementNextPageButtonClick);
			}
			#endregion

			#region Navigation
			AddImageTiled(87, 247, 1, 113, 2623); //Navigation Left Border
			AddImageTiled(88, 247, 108, 1, 2621); //Navigation Top Border
			AddImageTiled(88, 359, 108, 1, 2621); //Navigation Bottom Border
			AddImageTiled(196, 247, 1, 113, 2623); //Navigation Right Border

			//Navigation Buttons
			if (EditorState.SelectedElement != null)
			{
				AddButton(152, 296, 5601, 5605, b => onNavigateButtonClick(b, NavButtonDirection.Right));
				//NavRight Button
				AddButton(117, 296, 5603, 5607, b => onNavigateButtonClick(b, NavButtonDirection.Left));
				//NavLeft Button
				AddButton(135, 314, 5602, 5606, b => onNavigateButtonClick(b, NavButtonDirection.Down));
				//NavDown Button
				AddButton(135, 279, 5600, 5604, b => onNavigateButtonClick(b, NavButtonDirection.Up));
				//NavUp Button
				AddButton(092, 296, 5603, 5607, b => onNavigateButtonClick(b, NavButtonDirection.LeftLeft));
				//NavLeft3 Button
				AddButton(176, 296, 5601, 5605, b => onNavigateButtonClick(b, NavButtonDirection.RightRight));
				//NavRight3 Button
				AddButton(135, 339, 5602, 5606, b => onNavigateButtonClick(b, NavButtonDirection.DownDown));
				//NavDown3 Button
				AddButton(135, 255, 5600, 5604, b => onNavigateButtonClick(b, NavButtonDirection.UpUp));
				//NavUp3 Button
				AddButton(169, 296, 5601, 5605, b => onNavigateButtonClick(b, NavButtonDirection.RightRight));
				//NavRight2 Button
				AddButton(135, 332, 5602, 5606, b => onNavigateButtonClick(b, NavButtonDirection.DownDown));
				//NavDown2 Button
				AddButton(135, 262, 5600, 5604, b => onNavigateButtonClick(b, NavButtonDirection.UpUp));
				//NavUp2 Button
				AddButton(099, 296, 5603, 5607, b => onNavigateButtonClick(b, NavButtonDirection.LeftLeft));
				//NavLeft2 Button
			}
			else
			{
				AddImage(152, 296, 5601, 1101); //NavRight Button
				AddImage(117, 296, 5603, 1101); //NavLeft Button
				AddImage(135, 314, 5602, 1101); //NavDown Button
				AddImage(135, 279, 5600, 1101); //NavUp Button
				AddImage(092, 296, 5603, 1101); //NavLeft3 Button
				AddImage(176, 296, 5601, 1101); //NavRight3 Button
				AddImage(135, 339, 5602, 1101); //NavDown3 Button
				AddImage(135, 255, 5600, 1101); //NavUp3 Button
				AddImage(169, 296, 5601, 1101); //NavRight2 Button
				AddImage(135, 332, 5602, 1101); //NavDown2 Button
				AddImage(135, 262, 5600, 1101); //NavUp2 Button
				AddImage(099, 296, 5603, 1101); //NavLeft2 Button
			}

			AddButton(136, 295, GRID_BUTTON_ID, GRID_BUTTON_ID, b => onOpenGridButtonClick(b, 0));
			#endregion

			AddButton(250, 15, 5003, 5003, onGumpCloseButtonClick); //Browser Close Button
		}

		public void onOpenGridButtonClick(GumpButton button, object param)
		{
			EditorState.RendererToEdit.ShowEditorGrid = !EditorState.RendererToEdit.ShowEditorGrid;
			EditorState.Refresh();
		}

		public void XmlTextEntryBookCallback(Mobile from, object[] args, string response)
		{
			if (args.Length > 0 && args[0] is int)
			{
				var properties = EditorState.SelectedElement.GetElementPropertiesSnapshot();
				if (properties.Count > (int)args[0] && properties[(int)args[0]].BookCallback != null)
				{
					properties[(int)args[0]].BookCallback(response);
				}
			}

			EditorState.Refresh();
		}

		public void onBookEditTextButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var book = new XmlTextEntryBook(
					0,
					String.Empty,
					String.Empty,
					20,
					true,
					XmlTextEntryBookCallback,
					new object[] {param});

				var properties = EditorState.SelectedElement.GetElementPropertiesSnapshot();
				var bookText = properties[param].Text;
				// fill the contents of the book with the current text entry data
				book.FillTextEntryBook(bookText);

				// put the book at the location of the player so that it can be opened, but drop it below visible range
				book.Visible = false;
				book.Movable = false;
				book.MoveToWorld(
					new Point3D(EditorState.Caller.Location.X, EditorState.Caller.Location.Y, EditorState.Caller.Location.Z - 100),
					EditorState.Caller.Map);

				book.OnDoubleClick(EditorState.Caller);
			}

			EditorState.Refresh();
		}

		public virtual void onAddElementButtonClick(GumpButton button, int param)
		{
			if (button != null)
			{
				var instance = g_registeredElements.Values.First(registeredKvp => registeredKvp.Index == param);
				var element = instance.Method();
				element.Z = EditorState.RendererToEdit.Elements.Count;
				EditorState.RendererToEdit.Elements.Add(element);
				EditorState.RendererToEdit.SelectedElement = element;
				EditorState.SelectedElement = element;
				EditorState.ElementListGump.setPageBySelectedElement(element);
			}

			EditorState.Refresh();
		}

		public enum NavButtonDirection
		{
			Up = 1,
			UpUp = 2,
			Right = 3,
			RightRight = 4,
			Down = 5,
			DownDown = 6,
			Left = 7,
			LeftLeft = 8
		}

		public static void ShowSaveAs(object state)
		{
			var editorState = state as CompendiumEditorState;

			if (editorState != null)
			{
				editorState.Caller.SendGump(new SaveAsGump(editorState));
			}
		}

		public void onSaveAsButtonClick(GumpButton button)
		{
			EditorState.Refresh();
			Timer.DelayCall(ShowSaveAs, EditorState);
		}

		public void onApplyPropertiesButtonClick(GumpButton button)
		{
			EditorState.Refresh();
		}

		public void onElementNextPageButtonClick(GumpButton button)
		{
			EditorState.ElementToolbarPageNumber++;
			EditorState.Refresh();
		}

		public void onElementPrevPageButtonClick(GumpButton button)
		{
			EditorState.ElementToolbarPageNumber--;
			EditorState.Refresh();
		}

		public void onPropertyNextPageButtonClick(GumpButton button)
		{
			EditorState.PropertiesCurrentPageNumber++;
			EditorState.Refresh();
		}

		public void onPropertyPrevPageButtonClick(GumpButton button)
		{
			EditorState.PropertiesCurrentPageNumber--;
			EditorState.Refresh();
		}

		public void onSavePageButtonClick(GumpButton button)
		{
			EditorState.RendererToEdit.Serialize();

			if (Compendium.g_CompendiumRenderers.ContainsKey(EditorState.RendererToEdit.Name))
			{
				Compendium.g_CompendiumRenderers[EditorState.RendererToEdit.Name] = EditorState.RendererToEdit;
			}
			else
			{
				Compendium.g_CompendiumRenderers.Add(EditorState.RendererToEdit.Name, EditorState.RendererToEdit);
			}

			EditorState.Refresh();
		}

		public void onNavigateButtonClick(GumpButton button, NavButtonDirection dir)
		{
			if (EditorState == null)
			{
				return;
			}

			if (button != null && EditorState.SelectedElement != null)
			{
				switch (dir)
				{
					case NavButtonDirection.Up:
						EditorState.SelectedElement.Y--;
						break;
					case NavButtonDirection.UpUp:
						EditorState.SelectedElement.Y -= 5;
						break;
					case NavButtonDirection.Right:
						EditorState.SelectedElement.X++;
						break;
					case NavButtonDirection.RightRight:
						EditorState.SelectedElement.X += 5;
						break;
					case NavButtonDirection.Down:
						EditorState.SelectedElement.Y++;
						break;
					case NavButtonDirection.DownDown:
						EditorState.SelectedElement.Y += 5;
						break;
					case NavButtonDirection.Left:
						EditorState.SelectedElement.X--;
						break;
					case NavButtonDirection.LeftLeft:
						EditorState.SelectedElement.X -= 5;
						break;
				}
			}

			EditorState.Refresh();
		}

		public void onGumpCloseButtonClick(GumpButton button)
		{
			if (EditorState.Caller.HasGump(typeof(CompendiumPreviewPageGump)))
			{
				EditorState.Caller.CloseGump(typeof(CompendiumPreviewPageGump));
			}

			if (EditorState.Caller.HasGump(typeof(CompendiumPageEditor)))
			{
				EditorState.Caller.CloseGump(typeof(CompendiumPageEditor));
			}

			if (EditorState.Caller.HasGump(typeof(ElementListGump)))
			{
				EditorState.Caller.CloseGump(typeof(ElementListGump));
			}
		}
	}
}