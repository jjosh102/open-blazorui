
export function initializeChatHandlers() {
    const chatInput = document.getElementById('chat-input');
    if (chatInput) {
        chatInput.addEventListener('input', adjustChatWindowHeightAndScroll);
    }
}

export function adjustChatWindowHeightAndScroll() {
    const chatInput = document.getElementById('chat-input');
    const chatWindow = document.getElementById('chat-window');

    if (!chatInput || !chatWindow) return;

    chatInput.style.height = 'auto';
    const newInputHeight = Math.min(Math.max(chatInput.scrollHeight, 45), 200);
    chatInput.style.height = `${newInputHeight}px`;

    // Adjust textarea overflow
    chatInput.style.overflowY = newInputHeight >= 200 ? 'auto' : 'hidden';

    chatWindow.style.height = `calc(100vh - ${newInputHeight + 160}px)`; 
    scrollToBottom();
}

export function scrollToBottom() {
    const chatWindow = document.getElementById('chat-window');
    if (chatWindow) {
        requestAnimationFrame(() => {
            chatWindow.scrollTop = chatWindow.scrollHeight;
        });
    }
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
const handleResize = debounce(() => {
    adjustChatWindowHeightAndScroll();
}, 100);

window.addEventListener('resize', handleResize);