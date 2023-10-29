Feature: Create Booking
    In order to book a room
    As a customer
    I need to specify two dates to get a room in that time period
    
    Scenario: Booking is before the fully occupied date
        Given Fully booked date period starts in 7 days and ends in 14 days
        When The user makes a booking that starts in 1 days and ends in 6 days
        Then the booking is created
        