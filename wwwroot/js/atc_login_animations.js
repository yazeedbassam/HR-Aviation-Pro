// ATC Login Page Animations and Interactivity

document.addEventListener('DOMContentLoaded', function() {
    // Motivational Text Animation
    const motivationalText = document.getElementById('motivational-text');
    let offset = 0;
    const speed = 1; // Pixels per frame

    function animateText() {
        offset -= speed;
        motivationalText.style.left = offset + 'px';
        
        if (offset <= -motivationalText.parentElement.offsetWidth) {
            offset = motivationalText.parentElement.offsetWidth;
        }
        
        requestAnimationFrame(animateText);
    }
    
    animateText();
    
    // Panel Interactivity
    const infoPanels = document.querySelectorAll('.info-panel');
    const infoTriggers = document.querySelectorAll('.info-circle');
    const panelOverlay = document.querySelector('.panel-overlay');
    
    // Function to close all panels
    function closeAllPanels() {
        infoPanels.forEach(panel => {
            panel.classList.remove('active');
        });
        panelOverlay.classList.remove('active');
    }
    
    // Add click event to each trigger
    infoTriggers.forEach(trigger => {
        trigger.addEventListener('click', function() {
            const panelId = this.getAttribute('data-panel');
            const targetPanel = document.getElementById(panelId);
            
            // If panel is already active, close it
            if (targetPanel.classList.contains('active')) {
                targetPanel.classList.remove('active');
                panelOverlay.classList.remove('active');
            } else {
                // Close any open panels first
                closeAllPanels();
                
                // Open the clicked panel
                targetPanel.classList.add('active');
                panelOverlay.classList.add('active');
            }
        });
    });
    
    // Close panels when clicking overlay
    panelOverlay.addEventListener('click', closeAllPanels);
    
    // Control Tower Interaction
    const controlTower = document.getElementById('control-tower');
    const speechBubble = document.getElementById('speech-bubble');
    
    controlTower.addEventListener('click', function() {
        if (speechBubble.style.opacity === '1') {
            speechBubble.style.opacity = '0';
        } else {
            speechBubble.style.opacity = '1';
        }
    });
    
    // Contact Form Submission
    const contactForm = document.getElementById('contactForm');
    
    if (contactForm) {
        contactForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const name = document.getElementById('name').value;
            const email = document.getElementById('email').value;
            const message = document.getElementById('message').value;
            
            // Validate form
            if (!name || !email || !message) {
                alert('Please fill in all fields');
                return;
            }
            
            // Simulate sending email (in a real application, this would be an AJAX call to a server endpoint)
            alert(`Thank you, ${name}! Your message has been sent. We will contact you at ${email} soon.`);
            
            // Reset form
            contactForm.reset();
            
            // Close panel after successful submission
            closeAllPanels();
        });
    }
    
    // Radar Interactivity - Add random blips occasionally
    const radarScreen = document.querySelector('.radar-screen');
    
    if (radarScreen) {
        setInterval(function() {
            // Create a new blip at random position
            const blip = document.createElement('div');
            blip.className = 'radar-blip';
            
            // Random position within the radar
            const top = Math.random() * 80 + 10; // 10% to 90%
            const left = Math.random() * 80 + 10; // 10% to 90%
            
            blip.style.top = `${top}%`;
            blip.style.left = `${left}%`;
            
            radarScreen.appendChild(blip);
            
            // Remove the blip after animation
            setTimeout(function() {
                radarScreen.removeChild(blip);
            }, 4000);
        }, 3000); // Add new blip every 3 seconds
    }
});
