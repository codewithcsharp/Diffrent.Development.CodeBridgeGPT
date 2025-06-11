window.addEventListener('load', function () {
    const observer = new MutationObserver(function () {
        document.querySelectorAll('.model-title').forEach(title => {
            title.addEventListener('click', function () {
                const expanded = title.parentElement.querySelector('.model-box');
                if (expanded) {
                    const rightPanel = document.querySelector('#swagger-right-panel');
                    if (rightPanel) {
                        rightPanel.innerHTML = '';
                        rightPanel.appendChild(expanded.cloneNode(true));
                    }
                }
            });
        });
    });

    observer.observe(document.body, { childList: true, subtree: true });
});

window.addEventListener('DOMContentLoaded', () => {
    const rightPanel = document.createElement('div');
    rightPanel.id = 'swagger-right-panel';
    document.querySelector('#swagger-ui').appendChild(rightPanel);
});
