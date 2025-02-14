
//This is terrible , need to come up with a better one
window.adjustChatWindowHeightAndScroll = () => {
    const chatInput = document.getElementById('chat-input');
    const chatWindow = document.getElementById('chat-window');

    chatInput.style.height = 'auto';
    const newInputHeight = Math.min(chatInput.scrollHeight, 200);
    chatInput.style.height = `${newInputHeight}px`;

    chatWindow.style.height = `calc(100vh - 10rem - ${newInputHeight}px)`;
    chatInput.style.overflowY = newInputHeight >= 200 ? 'auto' : 'hidden';
}

export function scrollToBottom(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}
