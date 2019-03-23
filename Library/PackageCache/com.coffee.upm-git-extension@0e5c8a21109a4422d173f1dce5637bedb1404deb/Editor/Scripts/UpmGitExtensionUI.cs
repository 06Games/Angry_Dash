﻿using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Reflection;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Coffee.PackageManager
{
	[InitializeOnLoad]
	internal class UpmGitExtensionUI : VisualElement, IPackageManagerExtension
	{
		//################################
		// Constant or Static Members.
		//################################
#if UPM_GIT_EXT_PROJECT
		const string ResourcesPath = "Assets/UpmGitExtension/Editor/Resources/";
#else
		const string ResourcesPath = "Packages/com.coffee.upm-git-extension/Editor/Resources/";
#endif
		const string TemplatePath = ResourcesPath + "UpmGitExtension.uxml";
		const string StylePath = ResourcesPath + "UpmGitExtension.uss";

		static UpmGitExtensionUI ()
		{
			PackageManagerExtensions.RegisterExtension (new UpmGitExtensionUI ());
		}

		//################################
		// Public Members.
		//################################
		/// <summary>
		/// Creates the extension UI visual element.
		/// </summary>
		/// <returns>A visual element that represents the UI or null if none</returns>
		public VisualElement CreateExtensionUI ()
		{
			_initialized = false;
			return this;
		}

		/// <summary>
		/// Called by the Package Manager UI when a package is added or updated.
		/// </summary>
		/// <param name="packageInfo">The package information</param>
		public void OnPackageAddedOrUpdated (PackageInfo packageInfo)
		{
			if (_detailControls != null)
				_detailControls.SetEnabled (true);
		}

		/// <summary>
		/// Called by the Package Manager UI when a package is removed.
		/// </summary>
		/// <param name="packageInfo">The package information</param>
		public void OnPackageRemoved (PackageInfo packageInfo)
		{
			if (_detailControls != null)
				_detailControls.SetEnabled (true);
		}

		/// <summary>
		/// Called by the Package Manager UI when the package selection changed.
		/// </summary>
		/// <param name="packageInfo">The newly selected package information (can be null)</param>
		public void OnPackageSelectionChange (PackageInfo packageInfo)
		{
			InitializeUI ();
			if (!_initialized || packageInfo == null || _packageInfo == packageInfo)
				return;

			_packageInfo = packageInfo;

			var isGit = packageInfo.source == PackageSource.Git;

			UIUtils.SetElementDisplay (_gitDetailActoins, isGit);
			UIUtils.SetElementDisplay (_originalDetailActions, !isGit);
			UIUtils.SetElementDisplay (_detailControls.Q ("", "popupField"), !isGit);
			UIUtils.SetElementDisplay (_updateButton, isGit);
			UIUtils.SetElementDisplay (_versionPopup, isGit);
			UIUtils.SetElementDisplay (_originalAddButton, false);
			UIUtils.SetElementDisplay (_addButton, true);

			if (isGit)
			{
				_updateButton.text = "Update to";
				_versionPopup.SetEnabled (false);
				_updateButton.SetEnabled (false);
				GitUtils.GetRefs (PackageUtils.GetRepoHttpUrl (_packageInfo.packageId), _refs, () =>
				{
					_updateButton.SetEnabled (_currentRefName != _selectedRefName);
					_versionPopup.SetEnabled (true);
				});

				SetVersion (_currentRefName);
				EditorApplication.delayCall += () =>
				{
					UIUtils.SetElementDisplay (_detailControls.Q ("updateCombo"), true);
					UIUtils.SetElementDisplay (_detailControls.Q ("remove"), true);
					_detailControls.Q ("remove").SetEnabled (true);
				}
				;
				_currentHostData = Settings.GetHostData (_packageInfo.packageId);

				_hostingIcon.tooltip = "View on " + _currentHostData.Name;
				_hostingIcon.style.backgroundImage = EditorGUIUtility.isProSkin ? _currentHostData.LogoLight : _currentHostData.LogoDark;
			}
		}


		//################################
		// Private Members.
		//################################
		bool _initialized = false;
		PackageInfo _packageInfo;
		Button _hostingIcon { get { return _gitDetailActoins.Q<Button> ("hostingIcon"); } }
		Button _viewDocumentation { get { return _gitDetailActoins.Q<Button> ("viewDocumentation"); } }
		Button _viewChangelog { get { return _gitDetailActoins.Q<Button> ("viewChangelog"); } }
		Button _viewLicense { get { return _gitDetailActoins.Q<Button> ("viewLicense"); } }
		string _currentRefName { get { return PackageUtils.GetRefName (_packageInfo.packageId); } }
		string _selectedRefName { get { return _versionPopup.text != "(default)" ? _versionPopup.text : ""; } }
		VisualElement _detailControls;
		VisualElement _documentationContainer;
		VisualElement _originalDetailActions;
		VisualElement _gitDetailActoins;
		VisualElement _originalAddButton;
		VisualElement _addButton;
		Button _versionPopup;
		Button _updateButton;
		List<string> _tags = new List<string> ();
		List<string> _branches = new List<string> ();
		List<string> _refs = new List<string> ();
		HostData _currentHostData = null;



		/// <summary>
		/// Initializes UI.
		/// </summary>
		void InitializeUI ()
		{
			if (_initialized)
				return;

			var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset> (TemplatePath);
			if (!asset)
				return;

#if UNITY_2019_1_OR_NEWER
			_gitDetailActoins = asset.CloneTree().Q("detailActions");
            _gitDetailActoins.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet> (StylePath));
#else
			_gitDetailActoins = asset.CloneTree (null).Q ("detailActions");
			_gitDetailActoins.AddStyleSheetPath (StylePath);
#endif

			// Add callbacks
			_hostingIcon.clickable.clicked += () => Application.OpenURL (PackageUtils.GetRepoHttpUrl (_packageInfo));
			_viewDocumentation.clickable.clicked += () => Application.OpenURL (PackageUtils.GetFileURL (_packageInfo, "README.md"));
			_viewChangelog.clickable.clicked += () => Application.OpenURL (PackageUtils.GetFileURL (_packageInfo, "CHANGELOG.md"));
			_viewLicense.clickable.clicked += () => Application.OpenURL (PackageUtils.GetFileURL (_packageInfo, "LICENSE.md"));

			// Move element to documentationContainer
			_detailControls = parent.parent.Q ("detailsControls") ?? parent.parent.parent.parent.Q ("packageToolBar");
			_documentationContainer = parent.parent.Q ("documentationContainer");
			_originalDetailActions = _documentationContainer.Q ("detailActions");
			_documentationContainer.Add (_gitDetailActoins);

			_updateButton = new Button (AddOrUpdatePackage) { name = "update", text = "Up to date" };
			_updateButton.AddToClassList ("action");
			_versionPopup = new Button (PopupVersions);
			_versionPopup.AddToClassList ("popup");
			_versionPopup.AddToClassList ("popupField");
			_versionPopup.AddToClassList ("versions");

			if (_detailControls.name == "packageToolBar")
			{
				_hostingIcon.style.borderLeftWidth = 0;
				_hostingIcon.style.borderRightWidth = 0;
				_versionPopup.style.marginLeft = -10;
				_detailControls.Q ("rightItems").Insert (1, _updateButton);
				_detailControls.Q ("rightItems").Insert (2, _versionPopup);
			}
			else
			{
				_versionPopup.style.marginLeft = -4;
				_versionPopup.style.marginRight = -3;
				_versionPopup.style.marginTop = -3;
				_versionPopup.style.marginBottom = -3;
				_detailControls.Q ("updateCombo").Insert (1, _updateButton);
				_detailControls.Q ("updateDropdownContainer").Add (_versionPopup);
			}

			// Add package button
			var _root = UIUtils.GetRoot (_gitDetailActoins);
			_originalAddButton = _root.Q ("toolbarAddButton") ?? _root.Q ("moreAddOptionsButton");
			_addButton = new Button (AddPackage) { name = "moreAddOptionsButton", text = "+" };
			_addButton.AddToClassList ("toolbarButton");
			_addButton.AddToClassList ("space");
			_addButton.AddToClassList ("pulldown");
			_originalAddButton.parent.Insert (_originalAddButton.parent.IndexOf(_originalAddButton) + 1, _addButton);

			_initialized = true;
		}

		void PopupVersions ()
		{
			var menu = new GenericMenu ();
			var currentRefName = _currentRefName;

			menu.AddItem (new GUIContent (currentRefName + " - current"), _selectedRefName == currentRefName, SetVersion, currentRefName);

			// x.y(.z-sufix) only 
			foreach (var t in _refs.Where (x => Regex.IsMatch (x, "^\\d+\\.\\d+.*$")).OrderByDescending (x => x))
			{
				string target = t;
				bool isCurrent = currentRefName == target;
				GUIContent text = new GUIContent ("All Versions/" + (isCurrent ? target + " - current" : target));
				menu.AddItem (text, isCurrent, SetVersion, target);
			}

			// other 
			menu.AddItem (new GUIContent ("All Versions/Other/(default)"), _selectedRefName == "", SetVersion, "(default)");
			foreach (var t in _refs.Where (x => !Regex.IsMatch (x, "^\\d+\\.\\d+.*$")).OrderByDescending (x => x))
			{
				string target = t;
				bool isCurrent = currentRefName == target;
				GUIContent text = new GUIContent ("All Versions/Other/" + (isCurrent ? target + " - current" : target));
				menu.AddItem (text, isCurrent, SetVersion, target);
			}

			menu.DropDown (new Rect (_versionPopup.LocalToWorld (new Vector2 (0, 10)), Vector2.zero));
		}

		void SetVersion (object version)
		{
			string ver = version as string;
			_versionPopup.text = ver;
			_updateButton.SetEnabled (_currentRefName != _selectedRefName);
		}

		void AddOrUpdatePackage ()
		{
			var target = _versionPopup.text != "(default)" ? _versionPopup.text : "";
			var id = PackageUtils.GetSpecificPackageId (_packageInfo.packageId, target);
			PackageUtils.AddPackage (id);

			_versionPopup.SetEnabled (false);
			_updateButton.SetEnabled (false);
			_updateButton.text = "Updating to";
		}


		void AddPackage ()
		{
			var typePackage = Type.GetType ("UnityEditor.PackageManager.UI.Package, Unity.PackageManagerUI.Editor");
			var piAddRemoveOperationInProgress = typePackage.GetProperty ("AddRemoveOperationInProgress", BindingFlags.Static | BindingFlags.Public);
			var miAddFromLocalDisk = typePackage.GetMethod ("AddFromLocalDisk", BindingFlags.Static | BindingFlags.NonPublic);
			var addPackageFromDiskItem = new GUIContent ("Add package from disk...");
			var menu = new GenericMenu ();

			var menuPosition = _addButton.LocalToWorld (new Vector2 (_addButton.layout.width, 0));
			var menuRect = new Rect (menuPosition + new Vector2(0, -15), Vector2.zero);

			menu.AddItem (new GUIContent ("Add package from disk..."), false, delegate
			{
				var path = EditorUtility.OpenFilePanelWithFilters ("Select package on disk", "", new [] { "package.json file", "json" });
				if (!string.IsNullOrEmpty (path) && !(bool)piAddRemoveOperationInProgress.GetValue (null))
					miAddFromLocalDisk.Invoke (null, new object [] { path });
			});
			menu.AddItem (new GUIContent ("Add package from URL..."), false, delegate
			{
				EditorWindow.GetWindow<UpmGitAddWindow> (true);
			});

			menu.DropDown (menuRect);
		}

	}
}
