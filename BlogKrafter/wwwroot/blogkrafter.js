// Loaded as a module by BlogKrafter's editor screens.

function whenPickstackReady(timeoutMs) {
    const started = Date.now();
    return new Promise((resolve, reject) => {
        (function check() {
            if (window.Pickstack && window.Pickstack.files) {
                resolve(window.Pickstack.files);
            } else if (Date.now() - started > timeoutMs) {
                reject(new Error("Pickstack's script has not loaded on this page."));
            } else {
                setTimeout(check, 50);
            }
        })();
    });
}

// Opens the file manager the host registered, on the folder the host chose (empty for its upload folder).
// The choice comes back through the .NET callback, because closing the dialog without choosing
// never answers and an awaited call would wait for ever.
export async function pickFile(dotnet, method, type, root) {
    const files = await whenPickstackReady(5000);
    files.open({
        type: type || "any",
        root: root || "",
        callback: file => dotnet.invokeMethodAsync(method, file && file.url ? file.url : null)
    });
}

// Returns the new text for the component to keep; raising "change" as well would report it twice.
function commit(textarea, value, selectionStart, selectionEnd) {
    textarea.value = value;
    textarea.focus();
    textarea.setSelectionRange(selectionStart, selectionEnd);
    return value;
}

export function wrapSelection(textarea, before, after, placeholder) {
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = textarea.value.substring(start, end) || placeholder || "";
    const value = textarea.value.substring(0, start) + before + selected + after + textarea.value.substring(end);
    return commit(textarea, value, start + before.length, start + before.length + selected.length);
}

// Puts a prefix such as "## " or "> " at the start of every selected line.
export function prefixLines(textarea, prefix) {
    const text = textarea.value;
    const lineStart = text.lastIndexOf("\n", textarea.selectionStart - 1) + 1;
    const end = textarea.selectionEnd;
    const block = text.substring(lineStart, end);
    const prefixed = block.split("\n").map(line => prefix + line).join("\n");
    const value = text.substring(0, lineStart) + prefixed + text.substring(end);
    return commit(textarea, value, lineStart, lineStart + prefixed.length);
}

export function insertText(textarea, insert) {
    const start = textarea.selectionStart;
    const value = textarea.value.substring(0, start) + insert + textarea.value.substring(textarea.selectionEnd);
    return commit(textarea, value, start + insert.length, start + insert.length);
}

export function timeZone() {
    try {
        return Intl.DateTimeFormat().resolvedOptions().timeZone || null;
    } catch {
        return null;
    }
}
