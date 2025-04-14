

// تشغيل الفيديو عند الوقوف عليه
const videos = document.querySelectorAll('.news-card video');
videos.forEach(video => {
    video.addEventListener('mouseenter', () => {
        video.play();
    });
    video.addEventListener('mouseleave', () => {
        video.pause();
    });
});

document.addEventListener("DOMContentLoaded", function () {
    let content = document.querySelector(".news-detail-content");
    let readMoreBtn = document.querySelector(".read-more");

    if (content.scrollHeight > content.clientHeight) {
        readMoreBtn.style.display = "block"; // إظهار الزر إذا كان النص طويلًا
    }
});

function toggleReadMore() {
    let content = document.querySelector(".news-detail-content");
    let readMoreBtn = document.querySelector(".read-more");

    content.classList.toggle("expanded");

    if (content.classList.contains("expanded")) {
        readMoreBtn.textContent = "عرض أقل";
    } else {
        readMoreBtn.textContent = "عرض المزيد";
    }
}
