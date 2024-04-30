// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function encrypt(plainText, publicKey) {
    alert();
    // Convert the publicKey from XML to CryptoKey
    const publicKeyData = (new window.TextEncoder()).encode(publicKey);
    const publicKeyImported = await window.crypto.subtle.importKey(
        "spki",
        publicKeyData,
        {
            name: "RSA-OAEP",
            hash: {name: "SHA-256"}
        },
        true,
        ["encrypt"]
    );

    // Convert the plainText to ArrayBuffer
    const plainTextBuffer = (new window.TextEncoder()).encode(plainText).buffer;

    // Encrypt the plainText using the publicKey
    const encryptedBuffer = await window.crypto.subtle.encrypt(
        {
            name: "RSA-OAEP"
        },
        publicKeyImported,
        plainTextBuffer
    );

    // Convert the encryptedBuffer to byte array
    const encryptedArray = new Uint8Array(encryptedBuffer);

    alert( encryptedArray);
}
