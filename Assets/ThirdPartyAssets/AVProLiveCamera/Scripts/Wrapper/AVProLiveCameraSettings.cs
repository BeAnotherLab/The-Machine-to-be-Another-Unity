using UnityEngine;
using System.Text;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraSettingBase
	{
		protected int _deviceIndex;
		protected int _settingIndex;

		public enum DataType
		{
			Boolean,
			Float,
			Enum,
		}

		public DataType DataTypeValue
		{
			get;
			protected set;
		}

		public int PropertyIndex
		{
			get;
			protected set;
		}

		public string Name
		{
			get;
			protected set;
		}

		public bool CanAutomatic
		{
			get;
			protected set;
		}

		protected bool _isAutomatic;
		public bool IsAutomatic
		{
			get { return _isAutomatic; }
			set { if (value != _isAutomatic) IsDirty = true; _isAutomatic = value; }
		}

		public bool IsDirty
		{
			get;
			protected set;
		}

		public virtual void SetDefault()
		{

		}

		public virtual void Update()
		{

		}
	}

	public class AVProLiveCameraSettingBoolean : AVProLiveCameraSettingBase
	{
		private bool _currentValue;
		public bool CurrentValue
		{
			get { return _currentValue; }
			set { if (!_isAutomatic) { if (value != _currentValue) IsDirty = true; _currentValue = value; } }
		}

		public bool DefaultValue
		{
			get;
			private set;
		}

		public override void SetDefault()
		{
			CurrentValue = DefaultValue;
		}

		public override void Update()
		{
			float currentValue = _currentValue ? 1.0f : 0.0f;
			if (IsDirty)
			{
				AVProLiveCameraPlugin.ApplyDeviceVideoSettingValue(_deviceIndex, _settingIndex, currentValue, _isAutomatic);
				IsDirty = false;
			}
			else
			{
				AVProLiveCameraPlugin.UpdateDeviceVideoSettingValue(_deviceIndex, _settingIndex, out currentValue, out _isAutomatic);
				_currentValue = currentValue > 0.0f;
			}
		}

		public AVProLiveCameraSettingBoolean(int deviceIndex, int settingIndex, int propertyIndex, string name, bool canAutomatic, bool isAutomatic, bool defaultValue, bool currentValue)
		{
			_deviceIndex = deviceIndex;
			_settingIndex = settingIndex;
			DataTypeValue = DataType.Boolean;
			PropertyIndex = propertyIndex;
			Name = name;
			CanAutomatic = canAutomatic;

			IsAutomatic = isAutomatic;
			DefaultValue = defaultValue;
			CurrentValue = currentValue;

			IsDirty = false;
		}
	}

	public class AVProLiveCameraSettingFloat : AVProLiveCameraSettingBase
	{
		private float _currentValue;
		public float CurrentValue
		{
			get { return _currentValue; }
			set { if (!_isAutomatic) { if (value != _currentValue) IsDirty = true; _currentValue = value; } }
		}

		public float DefaultValue
		{
			get;
			private set;
		}
		public float MinValue
		{
			get;
			private set;
		}
		public float MaxValue
		{
			get;
			private set;
		}

		public float CurrentValueNormalised
		{
			set { CurrentValue = Mathf.Lerp(MinValue, MaxValue, value); }
			get { return (_currentValue - MinValue) / (MaxValue - MinValue); }
		}

		public override void SetDefault()
		{
			CurrentValue = DefaultValue;
		}

		public override void Update()
		{
			if (IsDirty)
			{
				AVProLiveCameraPlugin.ApplyDeviceVideoSettingValue(_deviceIndex, _settingIndex, _currentValue, _isAutomatic);
				IsDirty = false;
			}
			else
			{
				AVProLiveCameraPlugin.UpdateDeviceVideoSettingValue(_deviceIndex, _settingIndex, out _currentValue, out _isAutomatic);
			}
		}

		public AVProLiveCameraSettingFloat(int deviceIndex, int settingIndex, int propertyIndex, string name, bool canAutomatic, bool isAutomatic, float defaultValue, float currentValue, float minValue, float maxValue)
		{
			_deviceIndex = deviceIndex;
			_settingIndex = settingIndex;
			DataTypeValue = DataType.Float;
			PropertyIndex = propertyIndex;
			Name = name;
			CanAutomatic = canAutomatic;

			IsAutomatic = isAutomatic;
			MinValue = minValue;
			MaxValue = maxValue;
			DefaultValue = defaultValue;
			CurrentValue = currentValue;

			IsDirty = false;
		}
	}
}