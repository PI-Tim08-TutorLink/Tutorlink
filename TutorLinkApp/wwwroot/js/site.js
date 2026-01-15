document.addEventListener('DOMContentLoaded', function () {
    // Role selection for tutor skills
    const roleSelect = document.getElementById('roleSelect');
    const skillsGroup = document.getElementById('skillsGroup');
    const skillsWrapper = document.getElementById('skillsWrapper');

    function toggleSkills() {
        const targetElement = skillsGroup || skillsWrapper;
        if (!roleSelect || !targetElement) return;

        if (roleSelect.value === 'Tutor') {
            targetElement.style.display = 'block';
        } else {
            targetElement.style.display = 'none';
        }
    }

    if (roleSelect) {
        roleSelect.addEventListener('change', toggleSkills);
        toggleSkills(); // Initial check
    }
});
