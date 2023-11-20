Feature: Create Booking
    In order to book a room
    As a customer
    I need to specify two dates to get a room in that time period
    
    Scenario: Booking is before today
        Given there are no fully booked dates
        When the user makes a booking that starts in -7 days and ends in -1 days
        Then the booking is not created
        
    Scenario: Booking is on a unoccupied day
        Given there are no fully booked dates
        When the user makes a booking that starts in 8 days and ends in 9 days
        Then the booking is created
        
    Scenario: Booking is just before the fully occupied date
        Given Fully booked date period starts in 8 days and ends in 14 days
        When the user makes a booking that starts in 1 days and ends in 7 days
        Then the booking is created
        
    Scenario: Booking is inside the fully occupied date
        Given Fully booked date period starts in 1 days and ends in 9 days
        When the user makes a booking that starts in 1 days and ends in 9 days
        Then the booking is not created
        
    Scenario: Booking is just after the fully occupied date
        Given Fully booked date period starts in 1 days and ends in 7 days
        When the user makes a booking that starts in 8 days and ends in 11 days
        Then the booking is created
        
    Scenario: Booking is overlapping the fully occupied date
        Given Fully booked date period starts in 3 days and ends in 7 days
        When the user makes a booking that starts in 2 days and ends in 11 days
        Then the booking is created

    Scenario: Booking is overlapping occupied date's start day
        Given Fully booked date period starts in 3 days and ends in 7 days
        When the user makes a booking that starts in 2 days and ends in 5 days
        Then the booking is not created

    Scenario: Booking is overlapping the occupied date's end day
        Given Fully booked date period starts in 1 days and ends in 7 days
        When the user makes a booking that starts in 6 days and ends in 11 days
        Then the booking is not created
    
    Scenario Outline: Create bookings
        Given Fully booked date period starts in <BookedStart> days and ends in <BookedEnd> days
        When the user makes a booking that starts in <BookingStart> days and ends in <BookingEnd> days
        Then the booking is created: <Created>

        Examples:
          | BookedStart | BookedEnd | BookingStart | BookingEnd | Created |
          | 8           | 14        | 1            | 7          | true    |
          | 1           | 9         | 1            | 9          | false   |
          | 1           | 7         | 8            | 11         | true    |
          | 3           | 7         | 8            | 11         | true    |
          | 3           | 7         | 2            | 5          | false   |
          | 1           | 7         | 6            | 11         | false   |
    
        