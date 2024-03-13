(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.spkiHashes = exports.enabledLogging = exports.enabledNative = exports.resolver = exports.baseAddress = void 0;

const e = Module.findBaseAddress("Snapchat");

exports.baseAddress = e;
const s = new ApiResolver("objc");


const resolver = new ApiResolver('objc');

exports.resolver = s;

const o = !0;

exports.enabledNative = true;

const r = !1;

exports.enabledLogging = false;

const t = [ "mCBv6wZaYSYfYXqBmvJZDLrxuEzyBGWT2jA41pichMc=" ];

exports.spkiHashes = t;

},{}],2:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.hookSSL = void 0;

const e = require("./sectrust"), o = require("./boringssl"), S = require("./urlsession"), r = require("./snapchat");

function n() {
  e.hookSSL_SecTrustEvaluate(), e.hookSSL_SecTrustEvaluateWithError(), o.hookSSL_BoringSSL(), 
  o.hookSSL_BoringSSL_13(), S.hookSSL_URLSession(), r.hookSSL_Snapchat_EnableCertPinning(), 
  r.hookSSL_Snapchat_GetPinningEnabled(), r.hookSSL_Snapchat_PinnedCertsPublicKeyHashes();
}

exports.hookSSL = n;

},{"./boringssl":3,"./sectrust":4,"./snapchat":5,"./urlsession":6}],3:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.hookSSL_BoringSSL_13 = exports.hookSSL_BoringSSL = void 0;

const o = require("../globals");

let e = !1;

function n() {
  e || (e = !0, Module.load("/usr/lib/libboringssl.dylib"));
}

function t() {
  n();
  var e = Module.findExportByName("libboringssl.dylib", "SSL_CTX_set_custom_verify");
  if (null != e) {
    var t = new NativeFunction(e, "void", [ "pointer", "int", "pointer" ]), i = new NativeCallback((function(o, e) {
      return 0;
    }), "int", [ "pointer", "pointer" ]);
    Interceptor.replace(e, new NativeCallback((function(e, n, l) {
      o.enabledLogging && console.log("[*] Called CTXSetCustomVerify(...)."), t(e, n, i);
    }), "void", [ "pointer", "int", "pointer" ])), console.log("[*] Hooked SSL_CTX_set_custom_verify(...).");
  } else console.log("[!] Unable to find SSL_CTX_set_custom_verify(...)!");
}

function i() {
  n();
  const e = Module.findExportByName("libboringssl.dylib", "SSL_set_custom_verify");
  if (null == e) return void console.log("[!] Unable to find SSL_set_custom_verify(...)!");
  const t = new NativeFunction(e, "void", [ "pointer", "int", "pointer" ]), i = new NativeCallback((function(o, e) {
    return 0;
  }), "int", [ "pointer", "pointer" ]);
  Interceptor.replace(e, new NativeCallback((function(e, n, l) {
    o.enabledLogging && console.log("[*] Called SSL_set_custom_verify(...)."), t(e, n, i);
  }), "void", [ "pointer", "int", "pointer" ])), console.log("[*] Hooked SSL_set_custom_verify(...).");
}

exports.hookSSL_BoringSSL = t, exports.hookSSL_BoringSSL_13 = i;

},{"../globals":1}],4:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.hookSSL_SecTrustEvaluateWithError = exports.hookSSL_SecTrustEvaluate = void 0;

const e = require("../globals"), t = 0, r = 1, o = 3, n = 4, l = 5, a = 6, i = 7;

function u() {
  var t = Module.findExportByName("Security", "SecTrustEvaluate");
  if (null != t) {
    var r = new NativeFunction(t, "int", [ "pointer", "pointer" ]);
    Interceptor.replace(t, new NativeCallback((function(t, o) {
      e.enabledLogging && console.log("[*] Called SecTrustEvaluate(" + t + ", " + o + ").");
      r(t, o);
      return o.writeU8(4), 0;
    }), "int", [ "pointer", "pointer" ])), console.log("[*] Hooked SecTrustEvaluate(...).");
  } else console.log("[!] Unable to find SecTrustEvaluatePtr(...)!");
}

function c() {
  var t = Module.findExportByName("Security", "SecTrustEvaluateWithError");
  if (null != t) {
    var r = new NativeFunction(t, "int", [ "pointer", "pointer" ]);
    Interceptor.replace(t, new NativeCallback((function(t, o) {
      e.enabledLogging && console.log("[*] Called SecTrustEvaluateWithError(...).");
      r(t, o);
      return o.writePointer(ptr(0)), 1;
    }), "int", [ "pointer", "pointer" ])), console.log("[*] Hooked SecTrustEvaluateWithError(...).");
  } else console.log("[!] Unable to find SecTrustEvaluateWithErrorPtr(...)!");
}

exports.hookSSL_SecTrustEvaluate = u, exports.hookSSL_SecTrustEvaluateWithError = c;

},{"../globals":1}],5:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.hookSSL_Snapchat_PinnedCertsPublicKeyHashes = exports.hookSSL_Snapchat_GetPinningEnabled = exports.hookSSL_Snapchat_EnableCertPinning = void 0;

const e = require("../globals"), n = require("../utils");

function t() {
  let n = e.resolver.enumerateMatches("-[SCCrNetFrameworkController _enableCertPinning]");
  if (0 !== n.length) {
    for (var t = 0; t < n.length; t++) {
      var o = n[t];
      Interceptor.replace(o.address, new NativeCallback((function(e, n) {
        console.log("[*] Called [SCCrNetFrameworkController _enableCertPinning].");
      }), "void", [ "pointer", "pointer" ]));
    }
    console.log("[*] Hooked " + n.length + " -[SCCrNetFrameworkControllers _enableCertPinning].");
  } else console.log("[!] Unable to find -[SCCrNetFrameworkControllers _enableCertPinning]!");
}

function o() {
  let n = e.resolver.enumerateMatches("+[SCAPISecurityUtil getPinningEnabledHostsForCronetIncludeSubdomains]");
  if (0 !== n.length) {
    for (var t = 0; t < n.length; t++) {
      let e = n[t];
      Interceptor.replace(e.address, new NativeCallback((function(e, n) {
        return console.log("[*] Called +[SCAPISecurityUtil getPinningEnabledHostsForCronetIncludeSubdomains]."), 
        !1;
      }), "bool", []));
    }
    console.log("[*] Hooked " + n.length + " +[SCAPISecurityUtil getPinningEnabledHostsForCronetIncludeSubdomains].");
  } else console.log("[!] Unable to find +[SCRequestManager cronetConfig]");
}

function r() {
  let t = e.resolver.enumerateMatches("+[SCCertificateTrust pinnedCertsPublicKeyHashes]");
  if (0 !== t.length) {
    for (var o = 0; o < t.length; o++) {
      let r = 0, l = Instruction.parse(t[o].address), s = null, a = null;
      for (;;) {
        if ("adrp" == l.mnemonic && r++, 2 === r && null === s && (s = l), 4 === r) {
          a = l;
          break;
        }
        l = Instruction.parse(l.next);
      }
      let i = n.readAddressFromInstruction(s), c = n.readAddressFromInstruction(a).add("0x10").readPointer();
      Interceptor.attach(c, {
        onLeave: function(n) {
          let t = new ObjC.Object(i.readPointer());
          for (let n = 0; n < e.spkiHashes.length; n++) {
            const o = e.spkiHashes[n];
            let r = ObjC.classes.NSString.stringWithString_(o), l = ObjC.classes.NSData.dataWithBase64EncodedString_(r);
            t.addObject_(l);
          }
          console.log("[*] Injected SPKI hash into pinnedCertsPublicKeyHashes.");
        }
      });
    }
    console.log("[*] Hooked " + t.length + " +[SCCertificateTrust pinnedCertsPublicKeyHashes].");
  } else console.log("[!] Unable to find +[SCCertificateTrust pinnedCertsPublicKeyHashes]!");
}

exports.hookSSL_Snapchat_EnableCertPinning = t, exports.hookSSL_Snapchat_GetPinningEnabled = o, 
exports.hookSSL_Snapchat_PinnedCertsPublicKeyHashes = r;

},{"../globals":1,"../utils":7}],6:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.hookSSL_URLSession = void 0;

const e = require("../globals");

function n() {
  var n = ObjC.classes.NSURLCredential, o = e.resolver.enumerateMatches("-[* URLSession:didReceiveChallenge:completionHandler:]");
  if (0 !== o.length) {
    for (var t = 0; t < o.length; t++) {
      var l = o[t];
      Interceptor.attach(l.address, {
        onEnter: function(o) {
          var t = new ObjC.Object(o[0]), l = (ObjC.selectorAsString(o[1]), new ObjC.Object(o[3]));
          e.enabledLogging && console.log("[*] Called [" + t.$className + " URLSession:didReceiveChallenge:completionHandler:].");
          var s = new ObjC.Block(o[4]), r = s.implementation;
          s.implementation = function() {
            var e = n.credentialForTrust_(l.protectionSpace().serverTrust());
            l.sender().useCredential_forAuthenticationChallenge_(e, l), r(0, e);
          };
        }
      });
    }
    console.log("[*] Hooked " + o.length + " URLSessions.");
  } else console.log("[!] Unable to find [* URLSession:didReceiveChallenge:completionHandler:]!");
}

exports.hookSSL_URLSession = n;

},{"../globals":1}],7:[function(require,module,exports){
"use strict";

function n(n) {
  for (var r = {}, e = n.allKeys(), t = e.count(), o = 0; o < t; o++) {
    var i = e.objectAtIndex_(o), s = n.objectForKey_(i);
    r[i.toString()] = s.toString();
  }
  return r;
}

function r(n) {
  for (var r = [], e = n.count(), t = 0; t < e; t++) r[t] = n.objectAtIndex_(t).toString();
  return r;
}

function e() {
  return ObjC.available && "NSBundle" in ObjC.classes ? n(ObjC.classes.NSBundle.mainBundle().infoDictionary()) : null;
}

function t(e) {
  if (ObjC.available && "NSBundle" in ObjC.classes) {
    var t = ObjC.classes.NSBundle.mainBundle().infoDictionary().objectForKey_(e);
    return null === t ? t : "__NSCFArray" === t.class().toString() ? r(t) : "__NSCFDictionary" === t.class().toString() ? n(t) : t.toString();
  }
  return null;
}

function o(n) {
  return Array.prototype.map.call(new Uint8Array(n), (function(n) {
    return ("00" + n.toString(16)).slice(-2);
  })).join("");
}

function i(n) {
  for (var r = new Uint8Array(n.length / 2), e = 0; e < n.length; e += 2) r[e / 2] = parseInt(n.substring(e, e + 2), 16);
  return r.buffer;
}

function s(n) {
  let r, e = Instruction.parse(n.next), t = n.operands[1].value;
  if ("add" === e.mnemonic) r = e.operands[2].value; else {
    if ("ldr" !== e.mnemonic) throw Error("Unknown mnemonic " + e.mnemonic + " (" + JSON.stringify(e.groups) + ")");
    r = e.operands[1].value.disp;
  }
  return ptr(t + r);
}

Object.defineProperty(exports, "__esModule", {
  value: !0
}), exports.readAddressFromInstruction = exports.hex2buf = exports.buf2hex = exports.infoLookup = void 0, 
exports.infoLookup = t, exports.buf2hex = o, exports.hex2buf = i, exports.readAddressFromInstruction = s;

},{}],8:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: !0
});

const e = require("../../Frida_iOS/src/ssl/all");

e.hookSSL();

},{"../../Frida_iOS/src/ssl/all":2}]},{},[8]);
