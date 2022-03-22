//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace XDA {

public class XsString : XsStringImpl {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal XsString(global::System.IntPtr cPtr, bool cMemoryOwn) : base(xsensdeviceapiPINVOKE.XsString_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(XsString obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~XsString() {
    Dispose();
  }

  public override void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          xsensdeviceapiPINVOKE.delete_XsString(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
      base.Dispose();
    }
  }

  public XsString(uint sz, string src) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_0(sz, src), true) {
  }

  public XsString(uint sz) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_1(sz), true) {
  }

  public XsString() : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_2(), true) {
  }

  public XsString(XsStringImpl other) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_3(XsStringImpl.getCPtr(other)), true) {
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public XsString(string arg0, uint sz, XsDataFlags flags) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_4(arg0, sz, (int)flags), true) {
  }

  public XsString(string arg0, uint sz) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_5(arg0, sz), true) {
  }

  public XsString(string src) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_6(src), true) {
  }

  public XsString(SWIGTYPE_p_wchar_t src) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_7(SWIGTYPE_p_wchar_t.getCPtr(src)), true) {
  }

  public XsString(SWIGTYPE_p_std__wstring src) : this(xsensdeviceapiPINVOKE.new_XsString__SWIG_9(SWIGTYPE_p_std__wstring.getCPtr(src)), true) {
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public string c_str() {
    string ret = xsensdeviceapiPINVOKE.XsString_c_str__SWIG_0(swigCPtr);
    return ret;
  }

  public string toString() {
    string ret = xsensdeviceapiPINVOKE.XsString_toString(swigCPtr);
    return ret;
  }

  public SWIGTYPE_p_std__wstring toStdWString() {
    SWIGTYPE_p_std__wstring ret = new SWIGTYPE_p_std__wstring(xsensdeviceapiPINVOKE.XsString_toStdWString(swigCPtr), true);
    return ret;
  }

  new public XsString push_back(char c) {
    XsString ret = new XsString(xsensdeviceapiPINVOKE.XsString_push_back__SWIG_0(swigCPtr, c), false);
    return ret;
  }

  public new bool empty() {
    bool ret = xsensdeviceapiPINVOKE.XsString_empty(swigCPtr);
    return ret;
  }

  public new void append(XsStringImpl other) {
    xsensdeviceapiPINVOKE.XsString_append__SWIG_0_0(swigCPtr, XsStringImpl.getCPtr(other));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public void append(string other) {
    xsensdeviceapiPINVOKE.XsString_append__SWIG_1(swigCPtr, other);
  }

  public void append(SWIGTYPE_p_wchar_t other) {
    xsensdeviceapiPINVOKE.XsString_append__SWIG_2(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(other));
  }

  public uint utf8Len() {
    uint ret = xsensdeviceapiPINVOKE.XsString_utf8Len(swigCPtr);
    return ret;
  }

  public bool endsWith(XsString other, bool caseSensitive) {
    bool ret = xsensdeviceapiPINVOKE.XsString_endsWith__SWIG_0(swigCPtr, XsString.getCPtr(other), caseSensitive);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool endsWith(XsString other) {
    bool ret = xsensdeviceapiPINVOKE.XsString_endsWith__SWIG_1(swigCPtr, XsString.getCPtr(other));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool startsWith(XsString other, bool caseSensitive) {
    bool ret = xsensdeviceapiPINVOKE.XsString_startsWith__SWIG_0(swigCPtr, XsString.getCPtr(other), caseSensitive);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool startsWith(XsString other) {
    bool ret = xsensdeviceapiPINVOKE.XsString_startsWith__SWIG_1(swigCPtr, XsString.getCPtr(other));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool contains(XsString other, bool caseSensitive, SWIGTYPE_p_size_t offset) {
    bool ret = xsensdeviceapiPINVOKE.XsString_contains__SWIG_0(swigCPtr, XsString.getCPtr(other), caseSensitive, SWIGTYPE_p_size_t.getCPtr(offset));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool contains(XsString other, bool caseSensitive) {
    bool ret = xsensdeviceapiPINVOKE.XsString_contains__SWIG_1(swigCPtr, XsString.getCPtr(other), caseSensitive);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool contains(XsString other) {
    bool ret = xsensdeviceapiPINVOKE.XsString_contains__SWIG_2(swigCPtr, XsString.getCPtr(other));
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public SWIGTYPE_p_wchar_t utf8At(uint index) {
    SWIGTYPE_p_wchar_t ret = new SWIGTYPE_p_wchar_t(xsensdeviceapiPINVOKE.XsString_utf8At(swigCPtr, index), true);
    return ret;
  }

  public XsString push_back(SWIGTYPE_p_wchar_t c) {
    XsString ret = new XsString(xsensdeviceapiPINVOKE.XsString_push_back__SWIG_1(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(c)), false);
    if (xsensdeviceapiPINVOKE.SWIGPendingException.Pending) throw xsensdeviceapiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}