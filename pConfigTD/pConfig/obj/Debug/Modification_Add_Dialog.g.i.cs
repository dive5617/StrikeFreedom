﻿#pragma checksum "..\..\Modification_Add_Dialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "794940574E66FE534EBB96B3910A3683"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace pConfig {
    
    
    /// <summary>
    /// Modification_Add_Dialog
    /// </summary>
    public partial class Modification_Add_Dialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox name_txt;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox composition_txt;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox mass_txt;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox position_comboBox;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox site_txt;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox Neutral_Loss_txt;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\Modification_Add_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Common_checkBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/pConfig;component/modification_add_dialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Modification_Add_Dialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.name_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 7 "..\..\Modification_Add_Dialog.xaml"
            this.name_txt.PreviewGotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.txt_focus);
            
            #line default
            #line hidden
            return;
            case 2:
            this.composition_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 10 "..\..\Modification_Add_Dialog.xaml"
            this.composition_txt.PreviewGotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.txt_focus);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 11 "..\..\Modification_Add_Dialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Edit_btn_clk);
            
            #line default
            #line hidden
            return;
            case 4:
            this.mass_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 14 "..\..\Modification_Add_Dialog.xaml"
            this.mass_txt.PreviewGotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.txt_focus);
            
            #line default
            #line hidden
            return;
            case 5:
            this.position_comboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.site_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 24 "..\..\Modification_Add_Dialog.xaml"
            this.site_txt.PreviewGotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.txt_focus);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Neutral_Loss_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 26 "..\..\Modification_Add_Dialog.xaml"
            this.Neutral_Loss_txt.PreviewGotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.txt_focus);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 27 "..\..\Modification_Add_Dialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.apply_btn_clk);
            
            #line default
            #line hidden
            return;
            case 9:
            this.Common_checkBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

