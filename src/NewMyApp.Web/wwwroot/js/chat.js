let connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveMessage", function (message) {
    appendMessage(message);
    scrollToBottom();
});

connection.on("UserJoined", function (user) {
    updateOnlineUsers(user, true);
});

connection.on("UserLeft", function (user) {
    updateOnlineUsers(user, false);
});

async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error(err);
        setTimeout(startConnection, 5000);
    }
}

connection.onclose(async () => {
    await startConnection();
});

startConnection();

function appendMessage(message) {
    const messageContainer = document.createElement('div');
    messageContainer.className = 'message-container';
    messageContainer.setAttribute('data-message-id', message.id);

    const isCurrentUser = message.userId === currentUserId;
    messageContainer.classList.add(isCurrentUser ? 'message-right' : 'message-left');

    const messageContent = `
        <div class="message-avatar">
            <img src="${message.userAvatar}" alt="${message.userName}" class="rounded-circle">
        </div>
        <div class="message-content">
            <div class="message-header">
                <span class="message-author">${message.userName}</span>
                <span class="message-time">${message.createdAt}</span>
            </div>
            <div class="message-text">${message.content}</div>
        </div>
    `;

    messageContainer.innerHTML = messageContent;
    document.querySelector('#messagesList').appendChild(messageContainer);
}

function updateOnlineUsers(user, isOnline) {
    const userElement = document.querySelector(`[data-user-id="${user.id}"]`);
    if (userElement) {
        const statusDot = userElement.querySelector('.status-dot');
        if (isOnline) {
            statusDot.classList.add('online');
        } else {
            statusDot.classList.remove('online');
        }
    }
}

function scrollToBottom() {
    const messagesList = document.querySelector('#messagesList');
    messagesList.scrollTop = messagesList.scrollHeight;
}

document.querySelector('#messageInput').addEventListener('keypress', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

document.querySelector('#sendButton').addEventListener('click', sendMessage);

async function sendMessage() {
    const messageInput = document.querySelector('#messageInput');
    const message = messageInput.value.trim();
    const groupId = document.querySelector('#groupId').value;

    if (message) {
        try {
            await connection.invoke("SendMessage", parseInt(groupId), message);
            messageInput.value = '';
        } catch (err) {
            console.error(err);
        }
    }
}