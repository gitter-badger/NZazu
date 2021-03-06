using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NZazu.Contracts;
using NZazu.Contracts.Checks;
using NZazu.Extensions;
using NZazu.FieldBehavior;
using NZazu.Fields;

namespace NZazu
{
    public class NZazuView : ScrollViewer, INZazuWpfView
    {
        #region dependency properties

        // ############# FormDefinition

        public static readonly DependencyProperty FormDefinitionProperty = DependencyProperty.Register(
            "FormDefinition", typeof(FormDefinition), typeof(NZazuView), new PropertyMetadata(default(FormDefinition), FormDefinitionChanged));

        public FormDefinition FormDefinition
        {
            get { return (FormDefinition)GetValue(FormDefinitionProperty); }
            set { SetValue(FormDefinitionProperty, value); }
        }

        private static void FormDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (NZazuView)d;
            var formDefinition = (FormDefinition)e.NewValue;
            view.UpdateFields(formDefinition, view.FieldFactory, view.FieldBehaviorFactory, view.ResolveLayout);
            view.TrySetFocusOn(formDefinition.FocusOn);
        }

        // ############# FieldFactory

        public static readonly DependencyProperty FieldFactoryProperty = DependencyProperty.Register(
            "FieldFactory", typeof(INZazuWpfFieldFactory), typeof(NZazuView), new PropertyMetadata(new NZazuFieldFactory(), FieldFactoryChanged, FieldFactoryCoerceCallback));

        public INZazuWpfFieldFactory FieldFactory
        {
            get { return (INZazuWpfFieldFactory)GetValue(FieldFactoryProperty); }
            set { SetValue(FieldFactoryProperty, value); }
        }

        private static void FieldFactoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (NZazuView)d;
            var fieldFactory = (INZazuWpfFieldFactory)e.NewValue;
            view.UpdateFields(view.FormDefinition, fieldFactory, view.FieldBehaviorFactory, view.ResolveLayout);
        }

        private static object FieldFactoryCoerceCallback(DependencyObject d, object basevalue)
        {
            var view = (NZazuView)d;
            var fieldFactory = (INZazuWpfFieldFactory)basevalue;
            return fieldFactory ?? view.FieldFactory;
        }

        // ############# FieldBehaviorFactory

        public static readonly DependencyProperty FieldBehaviorFactoryProperty = DependencyProperty.Register(
            "FieldBehaviorFactory", typeof(INZazuWpfFieldBehaviorFactory), typeof(NZazuView),
            new PropertyMetadata(new NZazuFieldBehaviorFactory(),
                FieldBehaviorFactoryChanged, FieldBehaviorFactoryCoerceCallback));

        public INZazuWpfFieldBehaviorFactory FieldBehaviorFactory
        {
            get { return (INZazuWpfFieldBehaviorFactory)GetValue(FieldBehaviorFactoryProperty); }
            set { SetValue(FieldBehaviorFactoryProperty, value); }
        }

        private static void FieldBehaviorFactoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (NZazuView)d;
            var fieldBehaviorFactory = (INZazuWpfFieldBehaviorFactory)e.NewValue;
            view.UpdateFields(view.FormDefinition, view.FieldFactory, fieldBehaviorFactory, view.ResolveLayout);
        }

        private static object FieldBehaviorFactoryCoerceCallback(DependencyObject d, object basevalue)
        {
            var view = (NZazuView)d;
            var fieldBehaviorFactory = (INZazuWpfFieldBehaviorFactory)basevalue;
            return fieldBehaviorFactory ?? view.FieldBehaviorFactory;
        }

        // ############# ResolveLayout

        public static readonly DependencyProperty ResolveLayoutProperty = DependencyProperty.Register(
            "ResolveLayout", typeof(IResolveLayout), typeof(NZazuView),
            new PropertyMetadata(new ResolveLayout(), ResolveLayoutChanged, ResolveLayoutCoerceCallback));

        public IResolveLayout ResolveLayout
        {
            get { return (IResolveLayout)GetValue(ResolveLayoutProperty); }
            set { SetValue(ResolveLayoutProperty, value); }
        }

        private static void ResolveLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (NZazuView)d;
            var layoutStrategy = (IResolveLayout)e.NewValue;
            view.UpdateFields(view.FormDefinition, view.FieldFactory, view.FieldBehaviorFactory, layoutStrategy);
        }

        private static object ResolveLayoutCoerceCallback(DependencyObject d, object basevalue)
        {
            var view = (NZazuView)d;
            var layout = (IResolveLayout)basevalue;
            return layout ?? view.ResolveLayout;
        }

        // ############# FormData

        public static readonly DependencyProperty FormDataProperty = DependencyProperty.Register(
            "FormData", typeof(FormData), typeof(NZazuView),
            new PropertyMetadata(new FormData(), FormDataChanged));

        private static void FormDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (INZazuWpfView)d;
            var fieldValues = (FormData)e.NewValue;
            view.SetFieldValues(fieldValues.Values);
        }

        public FormData FormData
        {
            get { return (FormData)GetValue(FormDataProperty); }
            set { SetValue(FormDataProperty, value); }
        }


        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof (bool), typeof (NZazuView), new PropertyMetadata(default(bool), IsReadOnlyChanged));

        private static void IsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (NZazuView)d;
            var isReadOnly = (bool)e.NewValue;
            view.SetReadOnly(isReadOnly);
        }

        public bool IsReadOnly
        {
            get { return (bool) GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        #endregion

        public NZazuView()
        {
            InitializeComponent();
        }

        private ContentControl Layout => this;

        private void InitializeComponent()
        {
            Layout.Focusable = false;
            // cf.: http://compiledexperience.com/blog/posts/using-caliburn-micro-as-a-data-template-selector/
            Layout.IsTabStop = false;
            Layout.VerticalContentAlignment = VerticalAlignment.Stretch;
            Layout.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            SetHorizontalScrollBarVisibility(Layout, ScrollBarVisibility.Visible);
            SetVerticalScrollBarVisibility(Layout, ScrollBarVisibility.Visible);

            Layout.LostFocus += (s, e) => ApplyChanges();
        }

        public void ApplyChanges()
        {
            FormData = GetFieldValues();
        }

        public INZazuWpfField GetField(string key)
        {
            INZazuWpfField field;
            if (TryGetField(key, out field))
                return field;
            throw new KeyNotFoundException();
        }

        public bool TryGetField(string key, out INZazuWpfField field)
        {
            return _fields.TryGetValue(key, out field);
        }

        public Dictionary<string, string> GetFieldValues()
        {
            return _fields
                .Where(f => f.Value.IsEditable)
                .ToDictionary(f => f.Key, f => f.Value.StringValue);
        }

        public ValueCheckResult Validate()
        {
            var result = _fields.Values.Select(f => f.Validate()).FirstOrDefault(vr => !vr.IsValid);
            return result ?? ValueCheckResult.Success;
        }

        public bool TrySetFocusOn(string focusOn)
        {
            INZazuWpfField field;
            if (string.IsNullOrWhiteSpace(focusOn) || !TryGetField(focusOn, out field)) return false;

            var control = field.ValueControl;
            if (control == null) return false;

            control.SetFocus();
            control.DelayedFocus();

            return true;
        }

        private readonly IDictionary<string, INZazuWpfField> _fields = new Dictionary<string, INZazuWpfField>();

        private void UpdateFields(
            FormDefinition formDefinition,
            INZazuWpfFieldFactory fieldFactory,
            INZazuWpfFieldBehaviorFactory fieldBehaviorFactory,
            IResolveLayout resolveLayout)
        {
            DisposeFields();

            // make sure at least the minimum is set for render the layout
            if (formDefinition == null) return;
            if (formDefinition.Fields == null) return;

            CreateFields(formDefinition, fieldFactory);
            AttachBehavior(formDefinition, fieldBehaviorFactory);

            var layout = resolveLayout.Resolve(formDefinition.Layout);

            var parentFields = FormDefinition.Fields.Select(fd => GetField(fd.Key));
            layout.DoLayout(Layout, parentFields, resolveLayout);

            this.SetFieldValues(FormData.Values);

            SetReadOnly(IsReadOnly);
        }

        private void SetReadOnly(bool isReadOnly)
        {
            foreach (var field in _fields.Values)
                field.SetReadOnly(isReadOnly);
        }

        private void CreateFields(FormDefinition formDefinition, INZazuWpfFieldFactory fieldFactory)
        {
            formDefinition.Fields.ToList().ForEach(f =>
            {
                var field = fieldFactory.CreateField(f);
                _fields.Add(f.Key, field);
                AddGroupFieldKeys(field as INZazuWpfGroupField);
            });
        }

        private void AddGroupFieldKeys(INZazuWpfGroupField groupField)
        {
            if (groupField == null) return;
            foreach (var field in groupField.Fields)
            {
                _fields.Add(field.Key, field);
                AddGroupFieldKeys(field as INZazuWpfGroupField);
            }
        }

        private void DisposeFields()
        {
            DetachBehavior();
            _fields.Clear();
        }

        private void AttachBehavior(FormDefinition formDefinition, INZazuWpfFieldBehaviorFactory fieldBehaviorFactory)
        {
            formDefinition.Fields.ToList().ForEach(f => AttachBehavior(fieldBehaviorFactory, f));
        }

        private void AttachBehavior(INZazuWpfFieldBehaviorFactory fieldBehaviorFactory, FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.Behavior != null && !string.IsNullOrWhiteSpace(fieldDefinition.Behavior.Name))
            {
                var behavior = fieldBehaviorFactory.CreateFieldBehavior(fieldDefinition.Behavior);
                if (behavior == null) return;

                INZazuWpfField field;
                if (!TryGetField(fieldDefinition.Key, out field)) return;
                behavior.AttachTo(field, this);
                field.Behavior = behavior;
            }

            if (fieldDefinition.Fields == null) return;
            fieldDefinition.Fields.ToList().ForEach(f => AttachBehavior(fieldBehaviorFactory, f));
        }


        private void DetachBehavior()
        {
            DetachBehavior(_fields.Values);
        }

        private static void DetachBehavior(IEnumerable<INZazuWpfField> fields)
        {
            foreach (var field in fields)
            {
                if (field == null || field.Behavior == null) continue;
                field.Behavior.Detach();
                field.Behavior = null;
            }
        }
    }
}