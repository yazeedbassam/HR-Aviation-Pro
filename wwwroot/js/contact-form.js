// Enhanced Contact Form Interactivity
document.addEventListener('DOMContentLoaded', () => {
    // Get form elements
    const contactForm = document.getElementById('contactForm');
    const feedbackDiv = document.getElementById('contact-feedback');
    const submitButton = document.getElementById('sendEmailBtn');
    
    // Form validation and submission
    if(contactForm && feedbackDiv) {
        contactForm.addEventListener('submit', (event) => {
            event.preventDefault(); // Prevent actual submission
            
            // Basic form validation
            let isValid = true;
            const nameInput = document.getElementById('contact-name');
            const emailInput = document.getElementById('contact-email');
            const messageInput = document.getElementById('contact-message');
            
            // Reset previous validation
            [nameInput, emailInput, messageInput].forEach(input => {
                input.classList.remove('is-valid', 'is-invalid');
                const feedback = input.nextElementSibling;
                if (feedback && feedback.classList.contains('invalid-feedback')) {
                    feedback.remove();
                }
            });
            
            // Validate name
            if (!nameInput.value.trim()) {
                isValid = false;
                nameInput.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = 'Please enter your name';
                nameInput.parentNode.appendChild(feedback);
            } else {
                nameInput.classList.add('is-valid');
            }
            
            // Validate email
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailInput.value.trim() || !emailRegex.test(emailInput.value)) {
                isValid = false;
                emailInput.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = 'Please enter a valid email address';
                emailInput.parentNode.appendChild(feedback);
            } else {
                emailInput.classList.add('is-valid');
            }
            
            // Validate message
            if (!messageInput.value.trim()) {
                isValid = false;
                messageInput.classList.add('is-invalid');
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = 'Please enter your message';
                messageInput.parentNode.appendChild(feedback);
            } else {
                messageInput.classList.add('is-valid');
            }
            
            // If form is valid, simulate sending
            if (isValid) {
                // Show loading state
                submitButton.disabled = true;
                submitButton.classList.add('loading');
                
                // Update feedback div
                feedbackDiv.className = 'contact-feedback loading show';
                feedbackDiv.innerHTML = '<span class="loading-spinner"></span> Sending message...';
                
                // Simulate API call with timeout
                setTimeout(() => {
                    // Simulate success/failure (for demo purposes)
                    const isSuccess = Math.random() > 0.2; // 80% success rate for demo
                    
                    if (isSuccess) {
                        // Success feedback
                        feedbackDiv.className = 'contact-feedback success show';
                        feedbackDiv.textContent = 'Message sent successfully! We will contact you soon.';
                        contactForm.reset(); // Clear form
                        
                        // Reset validation classes
                        [nameInput, emailInput, messageInput].forEach(input => {
                            input.classList.remove('is-valid', 'is-invalid');
                        });
                    } else {
                        // Error feedback
                        feedbackDiv.className = 'contact-feedback error show';
                        feedbackDiv.textContent = 'Failed to send message. Please try again later.';
                    }
                    
                    // Reset button state
                    submitButton.disabled = false;
                    submitButton.classList.remove('loading');
                    
                    // Hide feedback after some time
                    setTimeout(() => {
                        feedbackDiv.classList.remove('show');
                    }, 5000);
                }, 1500);
            }
        });
        
        // Real-time validation as user types
        const inputs = contactForm.querySelectorAll('input, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                // Remove any existing feedback
                const existingFeedback = input.nextElementSibling;
                if (existingFeedback && existingFeedback.classList.contains('invalid-feedback')) {
                    existingFeedback.remove();
                }
                
                // Validate based on input type
                if (input.value.trim() === '') {
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                    
                    const feedback = document.createElement('div');
                    feedback.className = 'invalid-feedback';
                    feedback.textContent = `Please enter your ${input.name}`;
                    input.parentNode.appendChild(feedback);
                } else if (input.type === 'email') {
                    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                    if (!emailRegex.test(input.value)) {
                        input.classList.add('is-invalid');
                        input.classList.remove('is-valid');
                        
                        const feedback = document.createElement('div');
                        feedback.className = 'invalid-feedback';
                        feedback.textContent = 'Please enter a valid email address';
                        input.parentNode.appendChild(feedback);
                    } else {
                        input.classList.add('is-valid');
                        input.classList.remove('is-invalid');
                    }
                } else {
                    input.classList.add('is-valid');
                    input.classList.remove('is-invalid');
                }
            });
        });
    }
});
