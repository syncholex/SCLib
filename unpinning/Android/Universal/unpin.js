const certificate = "-----BEGIN CERTIFICATE-----\nMIIDNTCCAh2gAwIBAgIUHr5w333LoGNs6YMNFbf/HB7CtmAwDQYJKoZIhvcNAQEL\nBQAwKDESMBAGA1UEAwwJbWl0bXByb3h5MRIwEAYDVQQKDAltaXRtcHJveHkwHhcN\nMjMwNzE3MDIyNDI2WhcNMzMwNzE2MDIyNDI2WjAoMRIwEAYDVQQDDAltaXRtcHJv\neHkxEjAQBgNVBAoMCW1pdG1wcm94eTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC\nAQoCggEBALluLCRxoPkVw0mt2xHKgQg9luKNJWDqRooeLlFDYvlxZmRcTm6GefG2\nchGSlr81EOimn6hvVxx3L9FkCqgGbsK0MLOS1VP+uQ5BDMIG4NXjDRNYdSZ9BzyW\ngZo1ipR3Rs9Apy+6JZ1uDMtxWIlRSMlkzUng5WebyE9sBunOol5DpBUH+AoP7cvz\n0njEONZ+dwcnpecCdQg5bqkYrn6yDb+4t2YWp/w+S5U0fGwHMup03POw/oKNM6zq\ntIqVUHihQVxbnQxyB1swfkrxZcDNG0yjpJ/Ivc6IjRCsnCyMYtRAmb52BruWcDFU\nO6YvPWkcdfPyZkNldGevVUU9d6kHdfsCAwEAAaNXMFUwDwYDVR0TAQH/BAUwAwEB\n/zATBgNVHSUEDDAKBggrBgEFBQcDATAOBgNVHQ8BAf8EBAMCAQYwHQYDVR0OBBYE\nFMQ6zV8DCXz7fg2b7d9wOY7OPJISMA0GCSqGSIb3DQEBCwUAA4IBAQCdVFmn9DBQ\ntpGmuMDx3PBntg3cC6O/pB3uY8Kj812FRun/B91kQ5ziErxTNaVBqsXQUQimq7kg\nVJthO6SUKuaNxFVK7loyi6fWmNkvQotfgwIrOJt1e9Dk6xLKSN+zrLl8E9tafIBu\ntcq7qNOGv+0VtH9VCoa3ZkcF6RZvWJoO0wZolbWfPD1Y2yJ2L6utvo4S8OPu1Nry\n21YyRVu1dRAH3IRM+tARbJw8iyoaEEG822nsctGCZNJDN7HlkJ+a3j1icFEMtmPc\nb7EUHEIJM4SfkPg5V4tVla59Zmnb+QOVW58YlkpMtP4CaHA/Z27dlxvWgPM8X/wb\nuBrubjn/HW+b\n-----END CERTIFICATE-----\n"
Java.perform(function () {
	var SettingSecureClass = Java.use('android.provider.Settings$Secure');

	SettingSecureClass.getString.overload('android.content.ContentResolver', 'java.lang.String').implementation = function (context, name) {
		if (name.includes('android_id')) {
			var retval = this.getString(context, name);
			console.log("Invoke get androidId, with result : " + retval);

			var fake_value = "92ca0898227fec15"
			return fake_value;

		} else {
			var retval = this.getString(context, name);
			return retval;
		}
	};
});


// HTTP(S)
function hookSsl() {
    Java.perform(function () {
        console.log('[*] Script started');
    
        var certificateArray = Java.use('[Ljava.lang.String;');
        var JavaString = Java.use('java.lang.String');
        var myCertificate = JavaString.$new(certificate);
    
        var HookedClass = Java.use('java.security.cert.CertificateFactory');
        const InputStream = Java.use('java.io.ByteArrayInputStream');
        var inStreamCertificate = InputStream.$new(myCertificate.getBytes());
    
        var done = false;
    
        HookedClass.generateCertificate.implementation = function (inStream) {
            if(!done) {
                console.log("[*] Successfully changed the certificate");
                done = true;
                return this.generateCertificate(inStreamCertificate);
            }
    
            return this.generateCertificate(inStream);
        };

        console.log("[*] SSL pinning should be disabled.");	
    });
}

// Chat
function hookSslTwo() {
    Java.perform(function () {
        var SSLContext = Java.use("javax.net.ssl.SSLContext");
        var X509TrustManager = Java.use('javax.net.ssl.X509TrustManager');
        var TrustManager = Java.registerClass({
            name: 'com.sensepost.test.TrustManager',
            implements: [X509TrustManager],
            methods: {
                checkClientTrusted: function(chain, authType) {},
                checkServerTrusted: function(chain, authType) {},
                getAcceptedIssuers: function() {
                    return [];
                }
            }
        });
        var TrustManagers = [TrustManager.$new()];
        var SSLContext_init = SSLContext.init.overload('[Ljavax.net.ssl.KeyManager;', '[Ljavax.net.ssl.TrustManager;', 'java.security.SecureRandom');

        try {
            // Override the init method, specifying our new TrustManager
            SSLContext_init.implementation = function(keyManager, trustManager, secureRandom) {
                console.log("[+] Overriding SSLContext.init() with the custom TrustManager android < 7");
                SSLContext_init.call(this, keyManager, TrustManagers, secureRandom);
            };
        } catch (err) {
            console.log("[-] TrustManager Not Found");
        }
    });
}

hookSsl();
hookSslTwo();