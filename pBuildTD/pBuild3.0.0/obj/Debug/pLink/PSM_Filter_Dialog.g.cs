﻿#pragma checksum "..\..\..\pLink\PSM_Filter_Dialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "905AF41CFDECC947A7367D603482B267"
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
using pBuild.pLink;


namespace pBuild.pLink {
    
    
    /// <summary>
    /// PSM_Filter_Dialog
    /// </summary>
    public partial class PSM_Filter_Dialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 24 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox filter_mix_num;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox title_subStr_txt;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox sq_subStr_txt;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ratio1_tbx;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ratio2_tbx;
        
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
            System.Uri resourceLocater = new System.Uri("/pBuild;component/plink/psm_filter_dialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
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
            
            #line 8 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            ((pBuild.pLink.PSM_Filter_Dialog)(target)).Closed += new System.EventHandler(this.closed_event);
            
            #line default
            #line hidden
            return;
            case 2:
            this.filter_mix_num = ((System.Windows.Controls.TextBox)(target));
            
            #line 24 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            this.filter_mix_num.KeyDown += new System.Windows.Input.KeyEventHandler(this.enter_keyDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.title_subStr_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 30 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            this.title_subStr_txt.KeyDown += new System.Windows.Input.KeyEventHandler(this.enter_keyDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.sq_subStr_txt = ((System.Windows.Controls.TextBox)(target));
            
            #line 36 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            this.sq_subStr_txt.KeyDown += new System.Windows.Input.KeyEventHandler(this.enter_keyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ratio1_tbx = ((System.Windows.Controls.TextBox)(target));
            
            #line 42 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            this.ratio1_tbx.KeyDown += new System.Windows.Input.KeyEventHandler(this.enter_keyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ratio2_tbx = ((System.Windows.Controls.TextBox)(target));
            
            #line 48 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            this.ratio2_tbx.KeyDown += new System.Windows.Input.KeyEventHandler(this.enter_keyDown);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 53 "..\..\..\pLink\PSM_Filter_Dialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.filter_btn);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

