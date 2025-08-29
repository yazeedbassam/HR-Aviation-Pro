// Enhanced Panel Interactivity JavaScript
$(document).ready(function() {
    // Add close buttons to all panels
    $(".info-panel").each(function() {
        $(this).append('<div class="panel-close"></div>');
    });
    
    // Add overlay for closing panels when clicking outside
    $("body").append('<div class="panel-overlay"></div>');
    
    // Panel toggle functionality
    $(".info-circle").click(function() {
        const panelId = $(this).data("panel");
        
        // If the clicked panel is already open, close it
        if ($("#" + panelId).is(":visible")) {
            closePanel($("#" + panelId));
        } else {
            // Close any open panels first
            $(".info-panel:visible").each(function() {
                closePanel($(this));
            });
            
            // Show overlay
            $(".panel-overlay").fadeIn(300);
            
            // Show the selected panel with animation
            $("#" + panelId).css("display", "block");
            
            // Play radio chatter sound effect
            playRadioChatter();
        }
    });
    
    // Close panel when clicking the close button
    $(document).on("click", ".panel-close", function() {
        closePanel($(this).parent());
    });
    
    // Close panel when clicking outside
    $(".panel-overlay").click(function() {
        closePanel($(".info-panel:visible"));
    });
    
    // Function to close panel with animation
    function closePanel(panel) {
        panel.addClass("closing");
        setTimeout(function() {
            panel.removeClass("closing");
            panel.hide();
            $(".panel-overlay").fadeOut(300);
        }, 500);
        
        // Play radio sign-off sound effect
        playRadioSignoff();
    }
    
    // Sound effects for aviation theme
    function playRadioChatter() {
        // In a real implementation, this would play a short radio static/chatter sound
        console.log("Radio chatter sound effect");
    }
    
    function playRadioSignoff() {
        // In a real implementation, this would play a radio sign-off sound
        console.log("Radio sign-off sound effect");
    }
    
    // Add aviation-themed hover effects to panels
    $(".info-panel").hover(
        function() {
            $(this).css("box-shadow", "0 0 30px rgba(0, 123, 255, 0.3)");
        },
        function() {
            $(this).css("box-shadow", "0 0 30px rgba(0, 0, 0, 0.2)");
        }
    );
});
